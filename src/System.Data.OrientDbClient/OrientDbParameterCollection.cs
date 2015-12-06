using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;

namespace System.Data.OrientDbClient
{
    public class OrientDbParameterCollection : DbParameterCollection
    {
        private readonly List<OrientDbParameter> parameters = new List<OrientDbParameter>();
        private const string parameterPrefix = "$_";

        public override int Count => parameters.Count;

        public override object SyncRoot => ((ICollection)parameters).SyncRoot;

        public override int Add(object value)
        {
            OnChange();
            ValidateType(value);
            Validate(-1, value);
            parameters.Add((OrientDbParameter)value);
            return Count - 1;
        }

        public override void AddRange(Array values)
        {
            OnChange();
            if (null == values)
            {
                throw new ArgumentNullException("values");
            }
            foreach (object value in values)
            {
                ValidateType(value);
            }
            foreach (OrientDbParameter value in values)
            {
                Validate(-1, value);
                parameters.Add((OrientDbParameter)value);
            }
        }

        public override void Clear()
        {
            OnChange();
            parameters.Clear();
        }

        public override bool Contains(string value)
        {
            return (-1 != IndexOf(value));
        }

        public override bool Contains(object value)
        {
            return (-1 != IndexOf(value));
        }

        public override void CopyTo(Array array, int index)
        {
            ((System.Collections.ICollection)parameters).CopyTo(array, index);
        }

        public override IEnumerator GetEnumerator()
        {
            return ((System.Collections.ICollection)parameters).GetEnumerator();
        }

        public override int IndexOf(string parameterName)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].ParameterName == parameterName)
                    return i;
            }
            return -1;
        }

        public override int IndexOf(object value)
        {
            return parameters.IndexOf((OrientDbParameter)value);
        }

        public override void Insert(int index, object value)
        {
            OnChange();
            ValidateType(value);
            Validate(-1, (OrientDbParameter)value);
            parameters.Insert(index, (OrientDbParameter)value);
        }

        private void RangeCheck(int index)
        {
            if ((index < 0) || (Count <= index))
            {
                throw new IndexOutOfRangeException();
            }
        }

        public override void Remove(object value)
        {
            OnChange();
            ValidateType(value);
            int index = IndexOf(value);
            if (-1 != index)
            {
                RemoveAt(index);
            }
        }

        public override void RemoveAt(string parameterName)
        {
            OnChange();
            int index = IndexOf(parameterName);
            RemoveAt(index);
        }

        public override void RemoveAt(int index)
        {
            OnChange();
            RangeCheck(index);
            parameters.RemoveAt(index);
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            int index = IndexOf(parameterName);
            if (index < 0)
            {
                throw new ArgumentException(OrientDbStrings.ParameterNotFoundByName);
            }
            return parameters[index];
        }

        protected override DbParameter GetParameter(int index)
        {
            RangeCheck(index);
            return parameters[index];
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            OnChange();
            int index = IndexOf(parameterName);
            if (index < 0)
            {
                throw new ArgumentException(OrientDbStrings.ParameterNotFoundByName);
            }
            Replace(index, value);
        }

        public void Add(string parameterName, object value)
        {
            Add(new OrientDbParameter
            {
                ParameterName = parameterName,
                Value = value
            });
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            OnChange();
            RangeCheck(index);
            Replace(index, value);
        }

        private void Replace(int index, object newValue)
        {
            ValidateType(newValue);
            Validate(index, newValue);
            OrientDbParameter item = parameters[index];
            parameters[index] = (OrientDbParameter)newValue;
        }

        private void Validate(int index, object value)
        {
            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }
            
            String name = ((OrientDbParameter)value).ParameterName;
            if (name == null || 0 == name.Length)
            {
                index = 1;
                do
                {
                    name = parameterPrefix + index.ToString(CultureInfo.CurrentCulture);
                    index++;
                } while (-1 != IndexOf(name));
                ((OrientDbParameter)value).ParameterName = name;
            }
        }

        private void ValidateType(object value)
        {
            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }
            else if (!typeof(OrientDbParameter).IsInstanceOfType(value))
            {
                throw new ArgumentException(OrientDbStrings.ParametersMustBeOrientDbParameter);
            }
        }

        private void OnChange()
        {
        }

    }
}