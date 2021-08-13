using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xbim.Common;

using Bitub.Dto;

using Microsoft.Extensions.Logging;

namespace Bitub.Ifc.Transform
{
    public enum TransformAction
    {
        Transferred,
        NotTransferred,
        Modified,
        Added
    }

    public class TransformLogEntry
    {
        public readonly XbimInstanceHandle? InstanceHandle;
        public readonly TransformAction PerformedAction;

        internal protected TransformLogEntry(XbimInstanceHandle handle, TransformAction result)
        {
            InstanceHandle = handle;
            PerformedAction = result;
        }

        internal protected TransformLogEntry(TransformAction result)
        {
            PerformedAction = result;
        }
    }

    public class TransformPackage : IDisposable
    {
        public List<TransformLogEntry> Log { get; protected internal set; } = new List<TransformLogEntry>();

        public XbimInstanceHandleMap Map { get; private set; }

        public IModel Target 
        { 
            get => Map?.ToModel; 
        }

        public IModel Source 
        { 
            get => Map?.FromModel; 
        }
        
        public CancelableProgressing ProgressMonitor { get; protected set; }

        public bool IsCanceledOrBroken 
        {
            get => (null != ProgressMonitor) && (ProgressMonitor.State.IsCanceled || ProgressMonitor.State.IsBroken);
        }

        protected internal TransformPackage()
        {
        }

        protected internal TransformPackage(TransformPackage other)
        {
            Log = other.Log;
            Map = other.Map;
        }

        protected internal TransformPackage(XbimInstanceHandleMap map)
        {
            Map = map;
        }

        protected internal TransformPackage(IModel aSource, IModel aTarget, CancelableProgressing progressMonitor)
        {
            Map = new XbimInstanceHandleMap(aSource, aTarget);
            ProgressMonitor = progressMonitor;
        }

        public void Dispose()
        {
            Log?.Clear();
            Map?.Clear();
        }
    }

    public class TransformResult : TransformPackage
    {
        public enum Code
        {
            Finished, Canceled, ExitWithError
        }

        public readonly Code ResultCode;
        public string ResultMessage { get; internal set; }
        public Exception Cause { get; internal set; }

        internal TransformResult(Code r, TransformPackage package) : base(package)
        {
            ResultCode = r;
        }

        internal TransformResult(Code r, Exception exception = null) : base()
        {
            ResultCode = r;
            Cause = exception;
        }
    }

    /// <summary>
    /// Fundamental transformation request.
    /// </summary>
    public interface IIfcTransformRequest
    {
        /// <summary>
        /// The associated logger instance.
        /// </summary>
        ILogger Log { get; }

        /// <summary>
        /// A (unique) name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Runs the transformation request.
        /// </summary>
        /// <param name="aSource">The model</param>
        /// <param name="cancelableProgressing">An optional progress emitter</param>
        /// <returns></returns>
        Task<TransformResult> Run(IModel aSource, CancelableProgressing cancelableProgressing);
    }
}
