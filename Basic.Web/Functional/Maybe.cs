#region Includes
using System;
#endregion

namespace Basic.Web.Functional.Extensions
{
    internal static class ObjectExtensions
    {
        ///<summary>Converts a T value into a Maybe<TyValue>.Valid value.</summary>
        public static IMaybe<TyValue> Validate<TyValue>(this TyValue value)
         => Maybe.GetValid<TyValue>(value);

        ///<summary>Converts a T value into a Maybe<TyValue>.Valid value as long as the
        /// given condition is true otherwise it will return a
        /// Maybe.Invalid.</summary>
        public static IMaybe Validate<TyValue>(this TyValue value, bool condition)
         => Maybe.GetValid<TyValue>(value, condition);

        ///<summary>Converts a T value into a Maybe<TyValue>.Valid value if the
        /// comparer function returns true otherwise it will return a
        /// Maybe.Invalid.
        /// The comparer function can take one argument which is current
        /// value.</summary>
        public static IMaybe Validate<TyValue>(this TyValue value, Func<TyValue, bool> comparer)
         => Maybe.GetValid(value, comparer(value));

        ///<summary>Transforms a T value into a Maybe<TyNext>.Valid value as 
        /// long as the converter function returns a value of type TyNext,
        /// otherwise it will return Maybe.Invalid.
        /// The comparer function may take one argument which is current
        /// value.</summary>
        public static IMaybe Validate<TyValue,TyNext>(this TyValue value, Func<TyNext> valueConverter)
         => valueConverter() is TyNext next ? Maybe.GetValid(next) : Maybe.GetInvalid;

        ///<summary>Transforms a T value into a Maybe<TyNext>.Valid value as 
        /// long as the converter function returns a value of type TyNext,
        /// otherwise it will return Maybe.Invalid.
        /// The comparer function may take one argument which is current
        /// value.</summary>
        public static IMaybe Validate<TyValue,TyNext>(this TyValue value, Func<TyValue,TyNext> valueConverter)
         => valueConverter(value) is TyNext next ? Maybe.GetValid(next) : Maybe.GetInvalid;
    }

    internal static class IMaybeExtensions
    {
        ///<summary>Validate or Invalidates a Maybe value based on the given
        /// condition.</summary>
        public static IMaybe Validate(this IMaybe maybe, bool condition)
         => condition ? maybe : Maybe.GetInvalid;

        ///<summary>Validate or Invalidates a Maybe value based on output of the
        /// given comparer function.
        /// The comparer function takes one argument which is the Maybe wrapper
        /// itself which can be used to check the validity and retrieve the
        /// original value.</summary>
        public static IMaybe Validate(this IMaybe maybe, Func<IMaybe,bool> comparer)
         => comparer(maybe) ? maybe : Maybe.GetInvalid;
        
        ///<summary>Validate or Invalidates a Maybe value as long as the
        /// converter function returns a value of type TyNext, otherwise it will
        /// return Maybe.Invalid.
        /// The comparer function takes one argument which is the Maybe wrapper
        /// itself which can be used to check the validity and retrieve the
        /// original value.</summary>
        public static IMaybe Validate<TyNext>(this IMaybe maybe, Func<IMaybe,TyNext> valueConverter)
         => valueConverter(maybe) is TyNext next ? Maybe.GetValid(next) : Maybe.GetInvalid;
    }
}

namespace Basic.Web.Functional
{
    public interface IMaybe
    {
        Maybe.Invalid GetInvalid { get; }

        bool IsValid { get; }

        #region IMaybe Methods
        IMaybe Alternative<TyCast>(TyCast alternativeValue);

        IMaybe Alternative<TyCast>(IMaybe<TyCast> alternative);

        Maybe.Invalid AsInvalid();

        Maybe<TyCast>.Valid AsValid<TyCast>();

        ///<summary>Applies an action only when the current Maybe<TyValue> has a valid
        /// value.</summary>
        ///<return>The same object Maybe<TyValue> on which the action should be
        /// applied, regardless of whether the action as been run or not, making
        /// this method infinitely chainable.</return>
        IMaybe When<TyCase>(Action<TyCase> action);

        IMaybe When<TyCase,TyNext>(Func<TyCase,TyNext> valueConverter);

        IMaybe WhenInvalid(Action action);
        #endregion
    }

    public interface IMaybe<TyValue> : IMaybe
    {
        Maybe<TyValue>.Valid GetValid { get; }

        IMaybe<TyValue> WhenValid(Action<TyValue> action);

        IMaybe WhenValid<TyNext>(Func<TyValue,TyNext> mapper);
    }

    public static class Maybe
    {
        public sealed class Invalid : Maybe<object>
        {
            #region Maybe.Invalid Properties
            public override Invalid GetInvalid => this;
            #endregion

            public override Invalid AsInvalid() => this;
             
            public override IMaybe WhenInvalid(Action action)
            {
                action();
                return this;
            }
        }

        //TODO Verify the implication that this has in a multi-threaded context
        ///<summary>Static instance of Invalid object</summary>
        public static IMaybe GetInvalid { get; } = new Invalid();

        #region Maybe static Methods
        ///<summary>Converts a T value into a Maybe<TyValue>.Valid value.</summary>
        public static IMaybe<TyValue> GetValid<TyValue>(TyValue value)
         => new Maybe<TyValue>.Valid(value);

        ///<summary>Converts a T value into a Maybe<TyValue>.Valid value as long as the
        /// given condition is true otherwise it will return a
        /// Maybe.Invalid.</summary>
        public static IMaybe GetValid<TyValue>(TyValue value, bool condition)
         => condition ? Maybe.GetValid(value) : Maybe.GetInvalid;

        ///<summary>Converts a T value into a Maybe<TyValue>.Valid value if the
        /// parameterless comparer function returns true otherwise it will
        /// return a Maybe.Invalid.</summary>
        public static IMaybe GetValid<TyValue>(TyValue value, Func<bool> comparer)
         => Maybe.GetValid(value, comparer());

        ///<summary>Converts a T value into a Maybe<TyValue>.Valid value if the
        /// comparer function returns true otherwise it will return a
        /// Maybe.Invalid.
        /// The comparer function can take one argument which is current
        /// value.</summary>
        public static IMaybe GetValid<TyValue>(TyValue value, Func<TyValue, bool> comparer)
         => Maybe.GetValid(value, comparer(value));

        ///<summary>Transforms a T value into a Maybe<X>.Valid value as long as
        /// the converter function returns a value of type X, otherwise it will
        /// return Maybe<X>.Invalid.</summary>
        public static IMaybe GetValid<TyValue,TyNext>(TyValue value, Func<TyNext> valueConverter)
         => valueConverter() is TyNext next ? Maybe.GetValid(next) : Maybe.GetInvalid;

        ///<summary>Transforms a T value into a Maybe<X>.Valid value as long as
        /// the converter function returns a value of type X, otherwise it will
        /// return Maybe<X>.Invalid.
        /// The comparer function can take one argument which is current
        /// value.</summary>
        public static IMaybe GetValid<TyValue,TyNext>(TyValue value, Func<TyValue, TyNext> valueConverter)
         => valueConverter(value) is TyNext next ? Maybe.GetValid(next) : Maybe.GetInvalid;

        ///<summary>Converts a T value into a Maybe<TyValue>.Valid value as long as the
        /// value is a valid (not null) reference, otherwise it returns
        /// Maybe.Invalid</summary>
        public static IMaybe GetValid<TyValue>(ref TyValue reference)
         => Maybe.GetValid(reference, reference is not null);
        #endregion
    }

    public abstract class Maybe<TyValue> : IMaybe<TyValue>
    {
        public sealed class Valid : Maybe<TyValue>
        {
            public readonly TyValue Value;

            #region Maybe<TyValue>.Valid Properties
            public override bool IsValid => true;

            public override Maybe<TyValue>.Valid GetValid => this;
            #endregion

            public Valid(TyValue value) { this.Value = value; }

                
            public override IMaybe Alternative<TyCast>(IMaybe<TyCast> alternative) => this;
            public override IMaybe Alternative<TyCast>(TyCast alternativeValue) => this;

            public override Maybe<TyCast>.Valid AsValid<TyCast>()
             => this is Maybe<TyCast>.Valid valid
                ? valid
                : throw new InvalidOperationException($"Cannot convert an object of type {typeof(Maybe<TyValue>)} to {typeof(Maybe<TyCast>.Valid)} when the maybe has a valid value of type {typeof(TyValue)}.");
        
            public override IMaybe When<TyCase>(Action<TyCase> action)
            {
                if (this is Maybe<TyCase>.Valid valid) { action(valid.Value); }
                return this;
            }

            public override IMaybe When<TyCase,TyNext>(Func<TyCase,TyNext> valueConverter)
             => this is Maybe<TyCase>.Valid valid
                    ? Maybe.GetValid(valueConverter(valid.Value))
                    : this;

            public override IMaybe<TyValue> WhenValid(Action<TyValue> action)
            {
                action(this.Value);
                return this;
            }

            public override IMaybe WhenValid<TyNext>(Func<TyValue,TyNext> valueConverter)
             => Maybe.GetValid(valueConverter(this.Value));

        }

        #region Maybe<TyValue> Properties
        public virtual bool IsValid => false;

        public virtual Maybe.Invalid GetInvalid => null;
        public virtual Maybe<TyValue>.Valid GetValid => null;
        #endregion

        //NOTE Making this internal should somewhat limit inheritance from
        //  outside the library
        ///<summary>Default constructor.</summary>
        internal Maybe() {}

        public virtual IMaybe Alternative<TyCast>(IMaybe<TyCast> alternative)
         => (Maybe<TyCast>.Valid)alternative;

        public virtual IMaybe Alternative<TyCast>(TyCast alternativeValue)
         => Maybe.GetValid(alternativeValue);

        public virtual Maybe<TyCast>.Valid AsValid<TyCast>()
         => throw new InvalidOperationException($"Cannot convert an object of type {typeof(Maybe<TyValue>)} to {typeof(Valid)} when the maybe has no valid value.");

        public virtual Maybe.Invalid AsInvalid()
         => throw new InvalidOperationException($"Cannot convert an object of type {typeof(Maybe<TyValue>)} to {typeof(Maybe.Invalid)} when the maybe has a valid value");

        #region Maybe<TyValue> Methods
        public virtual IMaybe When<TyCase>(Action<TyCase> action) => this;
        public virtual IMaybe When<TyCase,TyNext>(Func<TyCase,TyNext> valueConverter) => this;

        public virtual IMaybe WhenInvalid(Action action) => this;
        
        public virtual IMaybe<TyValue> WhenValid(Action<TyValue> action) => this;

        public virtual IMaybe WhenValid<TyNext>(Func<TyValue,TyNext> valueConverter) => this;
        #endregion

        #region Maybe<TyValue> Operators
        //NOTE this cannot be statically extended, nor made virtual/abstract as of C# 9.0
        ///<summary>Implicit type conversion that wraps a naked value to a Maybe
        /// Monad</summary>
        public static implicit operator Maybe<TyValue>(TyValue value)
         => new Valid(value);
        #endregion
    }
}
