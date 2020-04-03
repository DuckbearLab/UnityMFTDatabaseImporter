using UnityEngine;
using System.Collections;
using System.Threading;
using System;
using DatabaseImporter.Runtime;

namespace DatabaseImporter.Importers
{

    public class ImportThread<T>
    {

        private object threadLock = new object();

        private bool done = false;

        private RuntimeTerrainImporter runOn;
        private Func<T> toRun;
        private Action<T> callback;
        private T result;

        public ImportThread(RuntimeTerrainImporter runOn, Func<T> toRun, Action<T> callback)
        {
            this.runOn = runOn;
            this.toRun = toRun;
            this.callback = callback;

            runOn.StartCoroutine(CheckIfDone());
        }

        public bool Done
        {
            get
            {
                lock (threadLock)
                {
                    return done;
                }
            }
            set
            {
                lock (threadLock)
                {
                    done = value;
                }
            }
        }

        public static void StartThread<T>(RuntimeTerrainImporter runOn, Func<T> toRun, Action<T> callback)
        {
            var importThread = new ImportThread<T>(runOn, toRun, callback);
            (new Thread(importThread.Start)).Start();

        }

        private void Start()
        {
            Done = false;
            result = toRun();
            Done = true;
        }

        private IEnumerator CheckIfDone()
        {
            while (!done)
                yield return null;
            while(runOn.AddedOnThisFrame > 0)
                yield return null;
            runOn.AddedOnThisFrame++;
            callback(result);
        }
    }

}