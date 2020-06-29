using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Transfer;

using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.IO.Step21;

namespace Bitub.Ifc.Transform.Requests
{
    /// <summary>
    /// A transform request template provided most of the functionality needed.
    /// <para>A typical transformation request will implemented the branching callback <see cref="PassInstance(IPersistEntity, T)"/>
    /// and if needed when using inverse relationships an override of <see cref="PropertyTransform(ExpressMetaProperty, object, T)"/>. More sophisticated processing
    /// is available by overriding <see cref="DelegateCopy(IPersistEntity, T)"/></para>
    /// </summary>
    /// <typeparam name="T">The package type</typeparam>
    public abstract class IfcTransformRequestTemplate<T> : IIfcTransformRequest where T : TransformPackage
    {
        public abstract string Name { get; }

        /// <summary>
        /// See <see cref="IIfcTransformRequest.IsInplaceTransform"/>
        /// </summary>
        public abstract bool IsInplaceTransform { get; }

        /// <summary>
        /// Whether the current transform doesn't do anything (based on configuration). This may imply
        /// that <see cref="IsInplaceTransform"/> will also yield to true.
        /// </summary>
        protected abstract bool IsNoopTransform { get; }

        public bool IsLogEnabled { get; set; } = true;

        /// <summary>
        /// The preferred storage type. If not set, source storage type will be used.
        /// </summary>
        public XbimStoreType? TargetStoreType { get; set; } 

        /// <summary>
        /// The editor credentials of change. If null, Xbim will use internal user identification.
        /// </summary>
        public XbimEditorCredentials EditorCredentials { get; set; }

        /// <summary>
        /// Transformation action type.
        /// </summary>
        protected enum TransformActionType
        {
            /// <summary>
            /// Drop the current instance (including relationsships).
            /// </summary>
            Drop, 
            /// <summary>
            /// Copy with direct relations.
            /// </summary>
            Copy, 
            /// <summary>
            /// Copy with inverse relations (including other instances depending on the current)
            /// </summary>
            CopyWithInverse, 
            /// <summary>
            /// Handle the copy process by delegation using <see cref="DelegateCopy(IPersistEntity, T)"/>.
            /// </summary>
            Delegate
        }

        /// <summary>
        /// Delegate handling the instance's property transformation. By default all properties (relations and data) are
        /// forwarded as they are into the transformation queue.
        /// </summary>
        /// <param name="property">The meta property descriptor</param>
        /// <param name="hostObject">The hosting object</param>
        /// <param name="package">The task's work package</param>
        /// <returns></returns>
        protected virtual object PropertyTransform(ExpressMetaProperty property, object hostObject, T package)
        {
            return property.PropertyInfo.GetValue(hostObject, null);
        }

        /// <summary>
        /// Whether to include an instance or not in transformation queue. Any dropped instance won't be injected into
        /// the new model.
        /// </summary>
        /// <param name="instance">An instance.</param>
        /// <param name="package">The task's work package</param>
        /// <returns>True, if to include into the transformation</returns>
        protected abstract TransformActionType PassInstance(IPersistEntity instance, T package);

        protected abstract T CreateTransformPackage(IModel aSource, IModel aTarget);

        protected virtual TransformResult.Code DoPreprocessTransform(T package, CancelableProgress progress)
        {
            return TransformResult.Code.Finished;
        }

        protected IEnumerable<E> EmptyToNull<E>(IEnumerable<E> elements)
        {
            if (elements.Any())
                return elements.ToArray();
            else
                return null;
        }

        protected IPersistEntity Copy(IPersistEntity instance, T package, bool withInverse)
        {
            package.Log?.Add(new TransformLogEntry(new XbimInstanceHandle(instance), TransformAction.Transferred));
            return package.Target.InsertCopy(instance, package.Map, (p, o) => PropertyTransform(p, o, package), withInverse, false);
        }

        protected virtual IPersistEntity DelegateCopy(IPersistEntity instance, T package)
        {
            return Copy(instance, package, true);
        }

        protected TransformResult.Code DoTransform(T package, CancelableProgress progress)
        {
            foreach (var instance in package.Source.Instances)
            {
                if (progress.State.IsCanceled)
                    return TransformResult.Code.Canceled;

                switch(PassInstance(instance, package))
                {
                    case TransformActionType.Copy:
                        Copy(instance, package, false);
                        break;
                    case TransformActionType.CopyWithInverse:
                        Copy(instance, package, true);
                        break;
                    case TransformActionType.Delegate:
                        DelegateCopy(instance, package);
                        break;
                    case TransformActionType.Drop:
                        package.Log?.Add(new TransformLogEntry(new XbimInstanceHandle(instance), TransformAction.NotTransferred));
                        break;
                }

                progress.NotifyProgress(1);
            }
            return TransformResult.Code.Finished;
        }

        protected virtual TransformResult.Code DoPostTransform(T package, CancelableProgress progress)
        {
            return TransformResult.Code.Finished;
        }

        private Xbim.IO.XbimStoreType DetectStorageType(IModel model)
        {
            if (model is IfcStore s)
                model = s.Model;
            if (model is Xbim.IO.Memory.MemoryModel)
                return XbimStoreType.InMemoryModel;
            else if (model is Xbim.IO.Esent.EsentModel)
                return XbimStoreType.EsentDatabase;
            else
                throw new NotSupportedException($"Unsupported / unknown type {model.GetType()}");
        }

        protected IfcStore CreateTargetModel(IModel sourcePattern, CancelableProgress progress)
        {
            var storeType = TargetStoreType ?? DetectStorageType(sourcePattern);

            progress.NotifyProgress($"Starting '{Name}' with '{storeType}' model implementation ...");
            IfcStore target;
            if (null == EditorCredentials)
                target = IfcStore.Create(sourcePattern.SchemaVersion, storeType);
            else
                target = IfcStore.Create(EditorCredentials, sourcePattern.SchemaVersion, storeType);

            // Set model factors
            target.ModelFactors.Initialise(
                sourcePattern.ModelFactors.AngleToRadiansConversionFactor,
                sourcePattern.ModelFactors.LengthToMetresConversionFactor,
                sourcePattern.ModelFactors.Precision);

            return target;
        }

        protected Func<TransformResult> FastForward(IModel source, CancelableProgress progress)
        {
            return () =>
            {
                progress.NotifyFinish($"Running fast forward '{Name}' model copy ...");
                return new TransformResult(TransformResult.Code.Finished, CreateTransformPackage(source, source));
            };
        }

        protected Func<TransformResult> PrepareInternally(IModel aSource, CancelableProgress progress)
        {
            Func<TransformResult> candidateResult = () =>
            {
                List<TransformLogEntry> logEntries = new List<TransformLogEntry>();
                IfcStore target = CreateTargetModel(aSource, progress);

                using (ITransaction txStore = target.BeginTransaction(Name))
                {
                    try
                    {
                        T package = CreateTransformPackage(aSource, target);
                        if (!IsLogEnabled)
                            package.Log = null;

                        TransformResult.Code code;
                        progress.NotifyProgress($"Preparing '{Name}' ...");
                        if (TransformResult.Code.Finished != (code = DoPreprocessTransform(package, progress)))
                            return new TransformResult(code);

                        progress.NotifyProgress($"Running '{Name}' ...");
                        if (TransformResult.Code.Finished != (code = DoTransform(package, progress)))
                            return new TransformResult(code);

                        progress.NotifyProgress($"Post processing '{Name}' ...");
                        if (TransformResult.Code.Finished != (code = DoPostTransform(package, progress)))
                            return new TransformResult(code);

                        txStore.Commit();
                        return new TransformResult(code, package);
                    }
                    catch (Exception e)
                    {
                        txStore.RollBack();
                        return new TransformResult(TransformResult.Code.ExitWithError, e);
                    }
                    finally
                    {
                        progress.NotifyFinish($"Transform '{Name}' has been finalized.");
                    }
                }
            };
            return candidateResult;
        }

        /// <summary>
        /// Creates a new transformation task but doesn't start it.
        /// </summary>
        /// <param name="aSource">The source model</param>
        /// <param name="progressing">The porgressing token</param>
        /// <returns></returns>
        public Task<TransformResult> Prepare(IModel aSource, out ICancelableProgressing progressing)
        {
            var progress = new CancelableProgress(null, aSource.Instances.Count);
            progressing = progress;
            if (IsNoopTransform)
                return new Task<TransformResult>(FastForward(aSource, progress));
            else
                return new Task<TransformResult>(PrepareInternally(aSource, progress));
        }

        /// <summary>
        /// Creates a new transformation task and starts it.
        /// </summary>
        /// <param name="aSource">The source model</param>
        /// <param name="progressReceiver">The progress receiver</param>
        /// <returns></returns>
        public Task<TransformResult> Run(IModel aSource, IProgress<ICancelableProgressState> progressReceiver)
        {
            var progress = new CancelableProgress(progressReceiver, aSource.Instances.Count);
            if (IsNoopTransform)
                return Task.Run(FastForward(aSource, progress));
            else
                return Task.Run(PrepareInternally(aSource, progress));
        }
    }
}
