namespace Biscuit
{
    public class Option
    {
        public static Option<T> Some<T>(T p)
        {
            return new Option<T>() { Value = p };
        }
    }

    public class Option<T>
    {
        public T Value { get; set; }

        public static Option<T> None()
        {
            return new Option<T>() { Value = default };
        }

        public static Option<T> Some(T p)
        {
            return new Option<T>() { Value = p };
        }

        public bool IsEmpty()
        {
            return Value == null;
        }

        public T Get()
        {
            return this.Value;
        }

        public bool IsDefined
        {
            get { return Value != null; }
        }

        public override bool Equals(object obj)
        {
            bool res = false;
            if(obj != null )
            {
                var isSubclass = this.Value.GetType().IsSubclassOf(obj.GetType().GenericTypeArguments[0]);
                if (isSubclass)
                {
                    object valueOfObj = obj.GetType().GetMethod("Get").Invoke(obj, null);
                    
                    if (valueOfObj != null)
                    {
                        res = valueOfObj.Equals(this.Value);
                    }
                }
            }
            return res;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Value);
        }
    }
}
