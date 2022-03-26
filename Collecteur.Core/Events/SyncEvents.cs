using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Collecteur.Core.Events
{
    public class SyncEvents
    {
        public SyncEvents()
        {

            _newItemEvent    = new AutoResetEvent(false);
            _exitThreadEvent = new ManualResetEvent(false);
            _BaseEchecThreadEvent = new ManualResetEvent(false);
            _EndInsertDataThreadEvent = new ManualResetEvent(false);
            _eventArray      = new WaitHandle[4];
            _eventArray[0]   = _newItemEvent;
            _eventArray[1]   = _exitThreadEvent;
            _eventArray[2] = _BaseEchecThreadEvent;
            _eventArray[3] = _EndInsertDataThreadEvent;
        }

        public EventWaitHandle ExitThreadEvent
        {
            get { return _exitThreadEvent; }
        }
        public EventWaitHandle BaseEchecThreadEvent
        {
            get { return _BaseEchecThreadEvent; }
        }
        public EventWaitHandle NewItemEvent
        {
            get { return _newItemEvent; }
        }
        public EventWaitHandle EndInsertDataThreadEvent
        {
            get { return _EndInsertDataThreadEvent; }
        }
        public WaitHandle[] EventArray
        {
            get { return _eventArray; }
        }

        private EventWaitHandle _newItemEvent;
        private EventWaitHandle _exitThreadEvent;
        private EventWaitHandle _BaseEchecThreadEvent;
        private EventWaitHandle _EndInsertDataThreadEvent;
        private WaitHandle[] _eventArray;
    }

}
