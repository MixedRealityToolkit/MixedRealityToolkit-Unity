// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Events;

// namespace Microsoft.MixedReality.SampleQRCodes
// {
//     public class QRCodesVisualizer : MonoBehaviour
//     {
//         public GameObject qrCodePrefab;
//         private GameObject currentQRCodeObject;  // 現在只追蹤一個QR碼物件

//         public struct ActionData
//         {
//             public enum Type
//             {
//                 Added,
//                 Updated,
//                 Removed
//             };
//             public Type type;
//             public Microsoft.MixedReality.QR.QRCode qrCode;

//             public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qrCode) : this()
//             {
//                 this.type = type;
//                 this.qrCode = qrCode;
//             }
//         }

//         public Queue<ActionData> pendingActions = new Queue<ActionData>();
//         public UnityEvent<string> OnStateUpdate;
//         public UnityEvent<string> OnUrl;
    

//         void Start()
//         {
//             Debug.Log("QRCodesVisualizer start");
//             currentQRCodeObject = null;
//             QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
//             QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
//             QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
//             QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;
//             if (qrCodePrefab == null)
//             {
//                 throw new System.Exception("Prefab not assigned");
//             }
//         }

//         private void Instance_QRCodesTrackingStateChanged(object sender, bool status)
//         {
//             if (!status && currentQRCodeObject != null)
//             {
//                 Destroy(currentQRCodeObject);
//                 currentQRCodeObject = null;
//             }
//         }

//         private void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
//         {
//             if (IsUrl(e.Data.Data))
//             {
//                 Debug.Log("QRCode URL: " + e.Data.Data + ", last update: "+ e.Data.LastDetectedTime.ToString("yyyy-MM-dd HH:mm:ss"));
//                 lock (pendingActions)
//                 {
//                     pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data));
//                 }
//             }
//         }

//         private void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
//         {
//             if (IsUrl(e.Data.Data))
//             {
//                 Debug.Log("QRCode URL Updated: " + e.Data.Data + ", last update: "+ e.Data.LastDetectedTime.ToString("yyyy-MM-dd HH:mm:ss"));
                
                
//                 lock (pendingActions)
//                 {
//                     pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data));
//                 }
//             }
//         }

//         private void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
//         {
//             lock (pendingActions)
//             {
//                 pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data));
//             }
//         }

//         private bool IsUrl(string data)
//         {
//             return data.StartsWith("http://") || data.StartsWith("https://");
//             // return Uri.TryCreate(data, UriKind.Absolute, out Uri uriResult)
//             //        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
//         }

//         private void HandleEvents()
//         {
//             lock (pendingActions)
//             {
//                 while (pendingActions.Count > 0)
//                 {
//                     var action = pendingActions.Dequeue();
//                     switch (action.type)
//                     {
//                         case ActionData.Type.Added:
//                             if (currentQRCodeObject == null && action.qrCode.LastDetectedTime > QRCodesManager.Instance.watcherStart)
//                             {
//                                 currentQRCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
//                                 currentQRCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
//                                 currentQRCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
//                                 OnStateUpdate.Invoke(new Qrstate(action.qrCode.Data, "Added").toState());
//                                 OnUrl.Invoke(action.qrCode.Data);
//                             }
//                             break;
//                         case ActionData.Type.Updated:
//                             if (currentQRCodeObject != null && currentQRCodeObject.GetComponent<QRCode>().qrCode.Id == action.qrCode.Id)
//                             {
//                                 // GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
//                                 // qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
//                                 // qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
//                                 OnStateUpdate.Invoke(new Qrstate(action.qrCode.Data, "Updated").toState());
//                                 // qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
//                             }
//                             break;
//                         case ActionData.Type.Removed:
//                             if (currentQRCodeObject != null && currentQRCodeObject.GetComponent<QRCode>().qrCode.Id == action.qrCode.Id)
//                             {
//                                 Destroy(currentQRCodeObject);
//                                 currentQRCodeObject = null;
//                                 OnStateUpdate.Invoke(new Qrstate(action.qrCode.Data, "Removed").toState());
//                             }
//                             break;
//                     }
//                 }
//             }
//         }

//         /// <summary>
//         /// Update is called every frame, if the MonoBehaviour is enabled.
//         /// </summary>
//         void Update()
//         {
//             HandleEvents();
//         }

//         public class Qrstate
//         {
//             public string url;
//             public string state;

//             public Qrstate(string url, string state)
//             {
//                 this.url = url;
//                 this.state = state;
//             }
//             public string toState(){
//                 string timestr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//                 return $"{state}\n{url}\n{timestr}";
//             }
//         }
//     }
// }

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace Microsoft.MixedReality.SampleQRCodes
{
    public class QRCodesVisualizer : MonoBehaviour
    {
        public GameObject qrCodePrefab;

        private SortedDictionary<System.Guid, GameObject> qrCodesObjectsList;
        private bool clearExisting = false;

        public UnityEvent<string> OnStateUpdate;
        public UnityEvent<string> OnUrl;

        public class Qrstate
        {
            public string url;
            public string state;

            public Qrstate(string url, string state)
            {
                this.url = url;
                this.state = state;
            }
            public string toState(){
                string timestr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                return $"{state}\n{url}\n{timestr}";
            }
        }

        public struct ActionData
        {
            public enum Type
            {
                Added,
                Updated,
                Removed
            };
            public Type type;
            public Microsoft.MixedReality.QR.QRCode qrCode;

            public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qRCode) : this()
            {
                this.type = type;
                qrCode = qRCode;
            }
        }

        public Queue<ActionData> pendingActions = new Queue<ActionData>();

        // Use this for initialization
        void Start()
        {
            Debug.Log("QRCodesVisualizer start");
            qrCodesObjectsList = new SortedDictionary<System.Guid, GameObject>();

            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;
            if (qrCodePrefab == null)
            {
                throw new System.Exception("Prefab not assigned");
            }
        }
        private void Instance_QRCodesTrackingStateChanged(object sender, bool status)
        {
            if (!status)
            {
                clearExisting = true;
            }
        }

        private void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeAdded");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data));
            }
        }

        private void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeUpdated");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data));
            }
        }

        private void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeRemoved");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data));
            }
        }

        private void HandleEvents()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    var action = pendingActions.Dequeue();
                    if (action.type == ActionData.Type.Added && action.qrCode.LastDetectedTime > QRCodesManager.Instance.watcherStart)
                    {
                        GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                        qrCodeObject.GetComponent<QRCode>().onURLFound.AddListener(OnUrl.Invoke);
                        qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                        
                        qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                        OnStateUpdate.Invoke(new Qrstate(action.qrCode.Data, "Added").toState());
                        
                    }
                    else if (action.type == ActionData.Type.Updated)
                    {
                        if (!qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                            qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                            qrCodeObject.GetComponent<QRCode>().onURLFound.AddListener(OnUrl.Invoke);
                            qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                            qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                            OnStateUpdate.Invoke(new Qrstate(action.qrCode.Data, "Added").toState());
                        }
                    }
                    else if (action.type == ActionData.Type.Removed)
                    {
                        if (qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            Destroy(qrCodesObjectsList[action.qrCode.Id]);
                            qrCodesObjectsList.Remove(action.qrCode.Id);
                            OnStateUpdate.Invoke(new Qrstate(action.qrCode.Data, "Removed").toState());
                        }
                    }
                }
            }
            if (clearExisting)
            {
                clearExisting = false;
                foreach (var obj in qrCodesObjectsList)
                {
                    Destroy(obj.Value);
                }
                qrCodesObjectsList.Clear();

            }
        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }
    }
}
