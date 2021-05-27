using System;

namespace Biscuit
{
    public class Option
    {
        public static Option<T> some<T>(T p)
        {
            return new Option<T>() { Value = p };
        }
    }

    public class Option<T>
    {
        public T Value { get; set; }

        public static Option<T> none()
        {
            return new Option<T>();
        }

        public static Option<T> some(T p)
        {
            return new Option<T>() { Value = p };
        }

        public bool isEmpty()
        {
            return Value == null;
        }

        public T get()
        {
            return this.Value;
        }

        public bool isDefined()
        {
            return Value != null;
        }

        public override bool Equals(object obj)
        {
            bool res = false;
            if(obj != null )
            {
                var isSubclass = this.Value.GetType().IsSubclassOf(obj.GetType().GenericTypeArguments[0]);
                ///obj.GetType().GenericTypeArguments[0].IsAssignableFrom(this.Value.GetType())
                if (isSubclass)
                {
                    object valueOfObj = obj.GetType().GetMethod("get").Invoke(obj, null);
                    // T == 
                    if (valueOfObj != null)
                    {
                        res = valueOfObj.Equals(this.Value);
                    }
                }
            }
            return res;
        }
    }
}
