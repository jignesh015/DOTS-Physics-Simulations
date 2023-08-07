using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsSimulations
{
    [Serializable]
    public class SerializableList<T>
    {
        public List<T> items;

        public SerializableList(List<T> list)
        {
            items = list;
        }
    }
}
