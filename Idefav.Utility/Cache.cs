using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Idefav.Utility
{
    internal delegate void EventSaveCache(object key, object value);
    public class Cache
    {
        protected Hashtable _Cache = new Hashtable();
        protected object _LockObj = new object();

        public int Count
        {
            get
            {
                return this._Cache.Count;
            }
        }

        public virtual object GetObject(object key)
        {
            if (this._Cache.ContainsKey(key))
                return this._Cache[key];
            return (object)null;
        }

        public void SaveCache(object key, object value)
        {
            new EventSaveCache(this.SetCache).BeginInvoke(key, value, new AsyncCallback(this.Results), (object)null);
        }

        protected virtual void SetCache(object key, object value)
        {
            lock (this._LockObj)
            {
                if (this._Cache.ContainsKey(key))
                    return;
                this._Cache.Add(key, value);
            }
        }

        private void Results(IAsyncResult ar)
        {
            ((EventSaveCache)((AsyncResult)ar).AsyncDelegate).EndInvoke(ar);
        }

        public virtual void DelObject(object key)
        {
            lock (this._Cache.SyncRoot)
              this._Cache.Remove(key);
        }

        public virtual void Clear()
        {
            lock (this._Cache.SyncRoot)
              this._Cache.Clear();
        }
    }
}
