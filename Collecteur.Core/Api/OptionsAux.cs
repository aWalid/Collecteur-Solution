using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XMLSerializer;

namespace Collecteur.Core.Api
{
    [Serializable]
    public class OptionsAux : Serialise, IEnumerable
    {
        private Dictionary<int, int?> _internalData;
        public Dictionary<int, int?> InternalData
        {
            get { return _internalData; }
            private set
            {
                _internalData = value;
            }
        }
        public OptionsAux()
        {
            _internalData = new Dictionary<int, int?>();
        }
        public int? GetOption(int keyValue)
        {
            if (keyValue != null)
            {
                if (_internalData.Keys.Contains(keyValue))
                    return _internalData[keyValue];
                else
                    return 0;
            }
            else
                return 0;
        }
        public int? this[int index]
        {
            get
            {
                if (index != null)
                {
                    if (_internalData.Keys.Contains(index))
                        return _internalData[index];
                    else
                        return null;
                }
                else
                    return null;
            }
            set
            {
                if (index != null)
                {
                    if (_internalData.Keys.Contains(index))
                        _internalData[index] = value;
                    else
                        _internalData.Add(index, value);
                }
            }

        }
        public int Length()
        {
            if (_internalData != null)
            {
                return _internalData.Count();
            }
            else
            {
                return 0;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _internalData.GetEnumerator();
        }
    }
}
