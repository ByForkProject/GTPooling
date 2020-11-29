﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GeoTetra.GTPooling
{
    [Obsolete("Use ServiceReferenceT")]
    [System.Serializable]
    public class ServiceReference : AssetReferenceT<GameObject>
    {
        public ServiceReference(string guid) : base(guid) { }

        internal ServiceReference(ServiceBehaviour service) : base(string.Empty)
        {
            _service = service;
        }

        private ServiceBehaviour _service;

        protected virtual void LoadServiceFromPool()
        {
            _service = string.IsNullOrEmpty(AssetGUID) ? 
                AddressableServicesPool.GlobalPool.PrePooledPopulate<ServiceBehaviour>() : 
                AddressableServicesPool.GlobalPool.PrePooledPopulate<ServiceBehaviour>(this);
            if (_service == null)
            {
                Debug.LogWarning($"{this.ToString()} Cannot find reference");
            }
        }

        public T Service<T>() where T : ServiceBehaviour
        {
            if (_service == null) LoadServiceFromPool();
            return (T) _service;
        }

        internal void SetService(ServiceBehaviour service)
        {
            _service = service;
        }
    }
    
    [System.Serializable]
    public class ServiceReferenceT<ServiceType> : AssetReferenceT<GameObject> where ServiceType : ServiceBehaviour
    {
        public ServiceReferenceT(string guid) : base(guid) { }

        private ServiceType _service;

        public ServiceType Service
        {
            get
            {
                if (_service == null) LoadServiceFromPool();
                return _service;
            }
            internal set => _service = value;
        }

        private void LoadServiceFromPool()
        {
            //If the Asset is not explicitly set, then it will try to load one that is named the same as the type.
            _service = string.IsNullOrEmpty(AssetGUID) ? 
                AddressableServicesPool.GlobalPool.PrePooledPopulate<ServiceType>() : 
                AddressableServicesPool.GlobalPool.PrePooledPopulate<ServiceType>(this);
        }
    }
    
    [System.Serializable]
    public class ServiceObjectReferenceT<ServiceObjectType> : AssetReferenceT<ServiceObjectType> where ServiceObjectType : ServiceObject
    {
        public ServiceObjectReferenceT(string guid) : base(guid) { }

        private ServiceObjectType _service;
        
        public ServiceObjectType Service
        {
            get
            {
                if (_service == null)
                {
                    Debug.Log($"Service not cached {typeof(ServiceObjectType).Name} call 'await .Cache()'.");
                }
                return _service;
            }
            internal set => _service = value;
        }
        
        /// <summary>
        /// Loads the reference for future Service calls.
        /// </summary>
        public async Task Cache()
        {
            if (_service == null) await LoadService();
        }

        private async Task LoadService()
        {
            if (string.IsNullOrEmpty(AssetGUID))
            {
                IResourceLocation location = AddressablesPoolUtility.GetResourceLocation<ServiceObjectType>(typeof(ServiceObjectType).Name);
                if (location != null)
                {
                    // Debug.Log("No ServiceObjectReference specified, loading default service by name of type for " + typeof(ServiceObjectType).Name);
                }
                else
                {
                    Debug.LogWarning("No ServiceObjectReference specified, and could not find a default service by name of type "  + typeof(ServiceObjectType).Name);
                }
                _service = await Addressables.LoadAssetAsync<ServiceObjectType>(location.PrimaryKey).Task;
            }
            else
            {
                _service = await LoadAssetAsync<ServiceObjectType>().Task;
            }

            await _service.Initialization;
        }
    }
}