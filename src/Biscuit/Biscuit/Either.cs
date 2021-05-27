using System;

namespace Biscuit
{
    /// <summary>
    /// Either represents a value of two possible data types. 
    /// An Either is either a Left or a Right. 
    /// By convention, the Left signifies a failure case result and the Right signifies a success.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public struct Either<T, U>
    {
        private readonly T left;
        private readonly U right;
        private readonly bool isLeft;

        public Either(T left)
        {
            this.left = left;
            this.right = default(U);
            this.isLeft = true;
        }

        public Either(U right)
        {
            this.left = default(T);
            this.right = right;
            this.isLeft = false;
        }

        public T Left { get { return left; } }

        public U Right { get { return right; } }

        public bool IsLeft { get { return this.isLeft; } }

        public bool IsRight { get { return !this.isLeft; } }

        public override bool Equals(object obj)
        {
            bool res = false;
            if (obj != null && (obj is Either<T, U>))
            {
                Either<T, U> x = (Either<T, U>)obj;
                if (x.IsLeft && this.IsLeft)
                {
                    res = Equals(x.Left, this.Left);
                    //if (x.Left == null && this.Left == null)
                    //{
                    //    res = true;
                    //}
                    //else if (x.Left != null && this.Left != null)
                    //{
                    //    res = x.Left.Equals(Left);
                    //}
                }
                else if (x.IsRight && this.IsRight)
                {
                    res = Equals(x.Right, this.Right);
                    //if (x.Right == null && this.Right == null)
                    //{
                    //    res = true;
                    //}
                    //else if (x.Right != null && this.Right != null)
                    //{
                    //    res = x.Right.Equals(this.Right);
                    //}                    
                }
            }
            return res;
        }

        public U Get()
        {
            return  Right;
        }

        public static implicit operator Either<T, U>(T value)
        {
            return new Either<T,U>(value);
        }

        public static implicit operator Either<T, U>(U value)
        {
            return new Either<T, U>(value);
        }

        public static implicit operator Either<T, U>(Left value)
        {
            return new Either<T, U>((T)value.Value);
        }

        public static implicit operator Either<T, U>(Right value)
        {
            return new Either<T, U>((U)value.Value);
        }

        /// <summary>
        /// Select
        /// </summary>
        public Either<T, UR> Select<UR>(Func<U, UR> selector)
        {
            if (selector == null) throw new ArgumentNullException("selector");

            if (IsLeft)
            {
                return left;
            }

            return selector(right);            
        }
    }

    public class Right
    {
        public object Value { get; set; }

        public Right(object o)
        {
            Value = o;
        }

        public override bool Equals(object obj)
        {
            var result = false;
            if (obj != null)
            {
                Type t = obj.GetType();
                bool isEither = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Either<,>);
                if (isEither)
                {
                    Type valueType = t.GetGenericArguments()[1];
                    var rightValue = t.GetProperty("Right").GetValue(obj);
                    
                    if (this.Value == null && rightValue == null)
                    {
                        result = true;
                    }
                    else if(this.Value.GetType() == valueType)
                    {
                        result = Value.Equals(rightValue);
                    }
                }
            }
            return result;
        }
    }

    public class Left
    {
        public object Value { get; set; }

        public Left(object o)
        {
            Value = o;
        }

        public override bool Equals(object obj)
        {
            var result = false;
            if (obj != null)
            {
                Type t = obj.GetType();
                bool isEither = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Either<,>);
                if (isEither)
                {
                    Type valueType = t.GetGenericArguments()[0];
                    var leftValue = t.GetProperty("Left").GetValue(obj);

                    if (this.Value == null && leftValue == null)
                    {
                        result = true;
                    }
                    else //if (this.Value.GetType()  valueType)
                    {
                        result = Value.Equals(leftValue);
                    }
                }
            }
            return result;
        }
    }
}
