using System.Threading;

namespace AmbientDataDemo.AmbientData
{
    public class MyAmbientDataAccessor : IMyAmbientDataAccessor
    {
        private static readonly AsyncLocal<MyAmbientDataHolder> _myAmbientDataCurrent = new AsyncLocal<MyAmbientDataHolder>();
        
        public MyAmbientData? MyAmbientData
        {
            get
            {
                return _myAmbientDataCurrent.Value?.Context;
            }
            set
            {
                var holder = _myAmbientDataCurrent.Value;
                if (holder != null)
                {
                    // Clear current HttpContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _myAmbientDataCurrent.Value = new MyAmbientDataHolder { Context = value };
                }
            }
        }

        private class MyAmbientDataHolder
        {
            public MyAmbientData? Context;
        }
    }
}