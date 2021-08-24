using System;
using System.Linq;
using System.Collections.Generic;

using Xbim.Common;

using Bitub.Dto;

namespace Bitub.Ifc.Transform
{
    /// <summary>
    /// Transformation action
    /// </summary>
    public enum TransformActionResult
    {
        /// <summary>
        /// Marks a copied instance of source.
        /// </summary>
        Copied = 0,
        /// <summary>
        /// Marks a skipped instance of source.
        /// </summary>
        Skipped = 1,
        /// <summary>
        /// Marks a partially copied or/and modified instance of source.
        /// </summary>
        Modified = 2,
        /// <summary>
        /// Marks a new instance of target.
        /// </summary>
        Added = 3
    }

    /// <summary>
    /// Performed transformation by affected instance handle and performed action.
    /// </summary>
    public sealed class TransformLogEntry
    {
        public readonly XbimInstanceHandle handle;
        public readonly TransformActionResult performed;

        internal TransformLogEntry(XbimInstanceHandle handle, TransformActionResult result)
        {
            this.handle = handle;
            this.performed = result;
        }

        internal TransformLogEntry(TransformActionResult result)
        {
            performed = result;
        }
    }

    /// <summary>
    /// Basic transformation package which bundles source and target model via an instance mapping map.
    /// </summary>
    public class TransformPackage : IDisposable
    {
        private List<TransformLogEntry> logEntry = new List<TransformLogEntry>();

        public IEnumerable<TransformLogEntry> Log { get => logEntry.ToArray(); }

        public XbimInstanceHandleMap Map { get; private set; }

        public ISet<TransformActionResult> LogFilter { get; } = new HashSet<TransformActionResult>();

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

        public bool LogAction(int sourceEntityLabel, TransformActionResult action)
        {
            return LogAction(new XbimInstanceHandle(Source, sourceEntityLabel), action);
        }

        public bool LogAction(XbimInstanceHandle sourceHandle, TransformActionResult action)
        {
            if (!LogFilter.Contains(action))
                return false;

            logEntry.Add(new TransformLogEntry(sourceHandle, action));
            return true;
        }

        protected TransformPackage(params TransformActionResult[] logFilter)
        {
            LogFilter = new HashSet<TransformActionResult>(logFilter);
        }

        protected internal TransformPackage(TransformPackage other, CancelableProgressing progressMonitor)
        {
            LogFilter = new HashSet<TransformActionResult>(other.LogFilter);
            logEntry = new List<TransformLogEntry>(other.logEntry);
            Map = other.Map;
            ProgressMonitor = progressMonitor;
        }

        protected internal TransformPackage(XbimInstanceHandleMap map, CancelableProgressing progressMonitor)
        {
            Map = map;
            ProgressMonitor = progressMonitor;
        }

        protected internal TransformPackage(IModel aSource, IModel aTarget, CancelableProgressing progressMonitor)
        {
            Map = new XbimInstanceHandleMap(aSource, aTarget);
            ProgressMonitor = progressMonitor;
        }

        public void Dispose()
        {
            if (null == Map)
                throw new ObjectDisposedException(ToString());

            logEntry.Clear();
            Map = null;
        }
    }
}
