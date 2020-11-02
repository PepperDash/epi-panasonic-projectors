using System;
using PepperDash_Essentials_Core.Queues;

namespace PepperDash.Essentials.Displays
{
    public class QueueMessage:IQueueMessage
    {
        private readonly Action _dispatchAction;

        public QueueMessage(Action dispatchAction)
        {
            _dispatchAction = dispatchAction;
        }

        public void Dispatch()
        {
            var handler = _dispatchAction;

            if (handler == null)
            {
                return;
            }

            handler();
        }
    }
}