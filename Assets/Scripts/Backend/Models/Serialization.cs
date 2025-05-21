using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orion.Backend.Models
{
    [Serializable]
    public class Serialization<T>
    {
        [SerializeField]
        private List<T> items;

        public Serialization()
        {
            items = new List<T>();
        }

        public Serialization(List<T> items)
        {
            this.items = items;
        }

        public List<T> ToList()
        {
            return items ?? new List<T>();
        }
    }
} 