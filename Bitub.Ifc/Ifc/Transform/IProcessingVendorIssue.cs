
using Xbim.Common;

using Bitub.Dto;

namespace Bitub.Ifc.Transform
{
    public interface IProcessingVendorIssue : IIfcTransformRequest
    {
        /// <summary>
        /// Indicates whether a model as the issue targeted by this processing transform or not.
        /// </summary>
        /// <param name="aSource">The model</param>
        /// <param name="cancelableProgressing">An optional progress emitter</param>
        /// <returns></returns>
        bool HasIssue(IModel aSource, CancelableProgressing cancelableProgressing);
    }
}
