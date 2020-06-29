using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Transfer;
using Xbim.Common;

namespace Bitub.Ifc.Transform
{
    public class IfcTransformQueue 
    {
        public event OnProgressChangeDelegate OnProgressChange;

        #region Internals
        private readonly CancelableProgressStateToken _sharedProgressState;
        private readonly List<IIfcTransformRequest> _requests;
        #endregion

        public IfcTransformQueue()
        {
            // New shared progress state with 100 percentage at maximum
            _sharedProgressState = new CancelableProgressStateToken(true, 100);
            _requests = new List<IIfcTransformRequest>();
        }

        public Task<TransformResult> Run(IModel aSource)
        {
            throw new NotImplementedException();
        }

        public int AppendRequest(IIfcTransformRequest request)
        {
            int index;
            lock (_requests)
            {
                index = _requests.Count;
                _requests.Add(request);
            }

            return index;
        }

        public IIfcTransformRequest this[int index]
        {
            get {
                lock(_requests)
                {
                    return _requests[index];
                }
            }
        }

        // Internal progress wrapper
        private class ProgressDetector : IProgress<CancelableProgressStateToken>
        {
            internal IfcTransformQueue ParentTransformQueue { get; set; }

            public void Report(CancelableProgressStateToken value)
            {
                ParentTransformQueue?.OnProgress(value);
            }
        }

        internal void OnProgress(CancelableProgressStateToken state)
        {
            OnProgressChange?.Invoke(state);
        }
    }
}
