# GenericArray
## 概要
C#でジェネリックな配列演算を提供します

## 使用方法
```
using System;
using GenericArray;

class Program
{
    static void Main(string[] args)
    {
        GArray<int> ga1 = new GArray<int>(1,2,3,4,5);
        GArray<double> ga2 = new GArray<double>(1.1, 2.2, 3.3, 4.4, 5.5);

        Console.WriteLine(ga1 + ga2); // [2.1, 4.2, 6.3, 8.4, 10.5]
    }
}
```

## クラス
- GArray\<T\>
    - 配列を表すクラスです。TはIConvertibleを継承する必要があります。
- UnConvertibleArray\<T\>
    - 同じく配列を表すクラスですが、キャストができません。
