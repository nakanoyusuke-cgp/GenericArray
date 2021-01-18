using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace GenericArray
{
    using Binary = Func<ParameterExpression, ParameterExpression, BinaryExpression>;

    /// <summary>
    /// IConvertibleを継承しない型はこちらで扱えます
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnConvertibleArray<T>
    {
        /// <summary>
        /// 配列初期化用デリゲート
        /// </summary>
        /// <param name="index">配列のインデックス</param>
        /// <returns>配列の[index]の位置に格納する値</returns>
        public delegate T Initializer(int index);

        /// <summary>
        /// 配列のサイズ
        /// </summary>
        public int Length => Array.Length;

        /// <summary>
        /// 配列本体
        /// </summary>
        public T[] Array { get; }

        /// <summary>
        /// 要素数を指定して初期化
        /// </summary>
        /// <param name="length">要素数</param>
        /// <param name="initializer">これを指定することでインデックスに応じた値の初期化ができる</param>
        public UnConvertibleArray(int length, Initializer initializer = null)
        {
            Array = new T[length];
            if (initializer != null)
                for (int i = 0; i < length; i++)
                    this[i] = initializer(i);
        }

        /// <summary>
        /// 配列を直接格納して初期化
        /// </summary>
        /// <param name="array">格納する配列</param>
        public UnConvertibleArray(params T[] array)
        {
            Array = new T[array.Length];
            System.Array.Copy(array, Array, Length);
        }

        /// <summary>
        /// IEnumerableを使って初期化
        /// </summary>
        /// <param name="iEnumerable">コレクションなど</param>
        public UnConvertibleArray(IEnumerable<T> iEnumerable)
        {
            var enumerable = iEnumerable as T[] ?? iEnumerable.ToArray();
            Array = new T[enumerable.Count()];
            System.Array.Copy(enumerable.ToArray(), Array, Length);
        }

        // インデクサ
        public T this[int index]
        {
            get => Array[index];
            set => Array[index] = value;
        }

        // 演算子オーバーロード
        public static UnConvertibleArray<T> operator +(UnConvertibleArray<T> left, UnConvertibleArray<T> right) => new UnConvertibleArray<T>(left.Length, i => Operator(Expression.Add)(left[i], right[i]));
        public static UnConvertibleArray<T> operator -(UnConvertibleArray<T> left, UnConvertibleArray<T> right) => new UnConvertibleArray<T>(left.Length, i => Operator(Expression.Subtract)(left[i], right[i]));
        public static UnConvertibleArray<T> operator *(UnConvertibleArray<T> left, UnConvertibleArray<T> right) => new UnConvertibleArray<T>(left.Length, i => Operator(Expression.Multiply)(left[i], right[i]));
        public static UnConvertibleArray<T> operator /(UnConvertibleArray<T> left, UnConvertibleArray<T> right) => new UnConvertibleArray<T>(left.Length, i => Operator(Expression.Divide)(left[i], right[i]));
        public static UnConvertibleArray<T> operator +(UnConvertibleArray<T> left, T right) => new UnConvertibleArray<T>(left.Length, i => Operator(Expression.Add)(left[i], right));
        public static UnConvertibleArray<T> operator -(UnConvertibleArray<T> left, T right) => new UnConvertibleArray<T>(left.Length, i => Operator(Expression.Subtract)(left[i], right));
        public static UnConvertibleArray<T> operator *(UnConvertibleArray<T> left, T right) => new UnConvertibleArray<T>(left.Length, i => Operator(Expression.Multiply)(left[i], right));
        public static UnConvertibleArray<T> operator /(UnConvertibleArray<T> left, T right) => new UnConvertibleArray<T>(left.Length, i => Operator(Expression.Divide)(left[i], right));

        // 演算関数呼び出し 無ければ生成してキャッシュに格納
        protected static Func<T, T, T> Operator(Binary op)
        {
            if (!OpDictionary.ContainsKey(op)) OpDictionary.Add(op, CreateOperator(op));
            return OpDictionary[op];
        }

        // 一度生成した演算関数のキャッシュ
        private static readonly Dictionary<Binary, Func<T, T, T>> OpDictionary = new Dictionary<Binary, Func<T, T, T>>();

        // 式木で演算関数を生成
        private static Func<T, T, T> CreateOperator(Binary op)
        {
            var left = Expression.Parameter(typeof(T), "left");
            var right = Expression.Parameter(typeof(T), "right");
            var expression = Expression.Lambda<Func<T, T, T>>(op(left, right), left, right);
            return expression.Compile();
        }

        //文字列化用のメソッド
        public override string ToString()
        {
            return "[" + string.Join(", ", Array) + "]";
        }

        /// <summary>
        /// foreachする用
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
                yield return Array[i];
        }
    }

    /// <summary>
    /// IConvertibleを継承した型のみ使用可能
    /// int,long,float,double,decimal,bool,string,char,byte,ulongへキャストが可能です。
    /// 浮動小数型のみ暗黙キャスト
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GArray<T> : UnConvertibleArray<T> where T : IConvertible
    {
        /// <summary>
        /// 要素数を指定して初期化
        /// </summary>
        /// <param name="length">要素数</param>
        /// <param name="initializer">これを指定することでインデックスに応じた値の初期化ができる</param>
        public GArray(int length, Initializer initializer = null) : base(length, initializer) { }

        /// <summary>
        /// 配列を直接格納して初期化
        /// </summary>
        /// <param name="array">格納する配列</param>
        public GArray(params T[] array) : base(array) { }

        /// <summary>
        /// IEnumerableを使って初期化
        /// </summary>
        /// <param name="iEnumerable">コレクションなど</param>
        public GArray(IEnumerable<T> iEnumerable) : base(iEnumerable) { }

        // ここから演算子オーバーロード
        public static implicit operator GArray<decimal>(GArray<T> array) => new GArray<decimal>(array.Length, i => Convert.ToDecimal(array[i]));
        public static implicit operator GArray<double>(GArray<T> array) => new GArray<double>(array.Length, i => Convert.ToDouble(array[i]));
        public static implicit operator GArray<float>(GArray<T> array) => new GArray<float>(array.Length, i => (float)Convert.ToDouble(array[i]));
        public static explicit operator GArray<bool>(GArray<T> array) => new GArray<bool>(array.Length, i => Convert.ToBoolean(array[i]));
        public static explicit operator GArray<int>(GArray<T> array) => new GArray<int>(array.Length, i => Convert.ToInt32(array[i]));
        public static explicit operator GArray<char>(GArray<T> array) => new GArray<char>(array.Length, i => Convert.ToChar(array[i]));
        public static explicit operator GArray<string>(GArray<T> array) => new GArray<string>(array.Length, i => Convert.ToString(array[i], CultureInfo.InvariantCulture));
        public static explicit operator GArray<byte>(GArray<T> array) => new GArray<byte>(array.Length, i => Convert.ToByte(array[i]));
        public static explicit operator GArray<ulong>(GArray<T> array) => new GArray<ulong>(array.Length, i => Convert.ToUInt64(array[i]));
        public static GArray<T> operator +(GArray<T> left, GArray<T> right) => new GArray<T>(left.Length, i => Operator(Expression.Add)(left[i], right[i]));
        public static GArray<T> operator -(GArray<T> left, GArray<T> right) => new GArray<T>(left.Length, i => Operator(Expression.Subtract)(left[i], right[i]));
        public static GArray<T> operator *(GArray<T> left, GArray<T> right) => new GArray<T>(left.Length, i => Operator(Expression.Multiply)(left[i], right[i]));
        public static GArray<T> operator /(GArray<T> left, GArray<T> right) => new GArray<T>(left.Length, i => Operator(Expression.Divide)(left[i], right[i]));
        public static GArray<T> operator +(GArray<T> left, T right) => new GArray<T>(left.Length, i => Operator(Expression.Add)(left[i], right));
        public static GArray<T> operator -(GArray<T> left, T right) => new GArray<T>(left.Length, i => Operator(Expression.Subtract)(left[i], right));
        public static GArray<T> operator *(GArray<T> left, T right) => new GArray<T>(left.Length, i => Operator(Expression.Multiply)(left[i], right));
        public static GArray<T> operator /(GArray<T> left, T right) => new GArray<T>(left.Length, i => Operator(Expression.Divide)(left[i], right));
    }
}