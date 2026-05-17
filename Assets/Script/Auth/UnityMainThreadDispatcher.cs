using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UnityMainThreadDispatcher : MonoBehaviour {

    private static readonly Queue<Action> m_ExecutionQueue = new Queue<Action>();

    private static UnityMainThreadDispatcher _instance = null;

    public static UnityMainThreadDispatcher Instance() {
        if (_instance == null) {
            _instance = FindFirstObjectByType<UnityMainThreadDispatcher>();
            if (_instance == null) {
                var go = new GameObject("UnityMainThreadDispatcher");
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
        }
        return _instance;
    }

    void Update() {
        lock (m_ExecutionQueue) {
            while (m_ExecutionQueue.Count > 0) {
                m_ExecutionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(IEnumerator action) {
        lock (m_ExecutionQueue) {
            m_ExecutionQueue.Enqueue(() => { StartCoroutine(action); });
        }
    }

    public void Enqueue(Action action) {
        Enqueue(ActionWrapper(action));
    }

    private IEnumerator ActionWrapper(Action action) {
        action();
        yield return null;
    }
}