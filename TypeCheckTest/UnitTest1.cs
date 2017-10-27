using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using D_Sharp;
using System.Collections.Generic;

namespace TypeCheckTest
{


    class SuperClass
    {

    }

    class SubClass : SuperClass
    {

    }

    [TestClass]
    public class UnitTest1
    {
        public static void Foo<T>(Dictionary<int,T> d)
        {

        }

            [TestMethod]
        public void TestTypeMatch()
        {
            //TypeMatchのテスト
            var hoge = TypeCheck.GetParamsPriority(typeof(int), typeof(int));
            Assert.AreEqual(ParamsPriority.MatchKind.TypeMatch,hoge.matchkind);

            hoge = TypeCheck.GetParamsPriority(typeof(float), typeof(float));
            Assert.AreEqual(ParamsPriority.MatchKind.TypeMatch, hoge.matchkind);

            //ImplicitCastMatchのテスト
            hoge = TypeCheck.GetParamsPriority(typeof(float), typeof(double));
            Assert.AreEqual(ParamsPriority.MatchKind.ImplicitCastMatch, hoge.matchkind);

            //UpCastMatch
            hoge = TypeCheck.GetParamsPriority(typeof(bool), typeof(object));
            Assert.AreEqual(ParamsPriority.MatchKind.UpCastMatch, hoge.matchkind);
            Assert.AreEqual(2, hoge.upCastNest);

            hoge = TypeCheck.GetParamsPriority(typeof(SubClass), typeof(SuperClass));
            Assert.AreEqual(ParamsPriority.MatchKind.UpCastMatch, hoge.matchkind);
            Assert.AreEqual(1, hoge.upCastNest);

            //GenericTypeMatch
            hoge = TypeCheck.GetParamsPriority(typeof(float), typeof(List<>).GetGenericArguments()[0]);
            Assert.AreEqual(ParamsPriority.MatchKind.GenericTypeMatch, hoge.matchkind);
            Assert.AreEqual(1, hoge.concreteness);

            hoge = TypeCheck.GetParamsPriority(typeof(List<int>), typeof(List<>));
            Assert.AreEqual(ParamsPriority.MatchKind.GenericTypeMatch, hoge.matchkind);
            Assert.AreEqual(2, hoge.concreteness);

            hoge = TypeCheck.GetParamsPriority(typeof(Dictionary<int,double>), typeof(Dictionary<,>));
            Assert.AreEqual(ParamsPriority.MatchKind.GenericTypeMatch, hoge.matchkind);
            Assert.AreEqual(3, hoge.concreteness);

            hoge = TypeCheck.GetParamsPriority(typeof(Dictionary<int, double>), typeof(UnitTest1).GetMethod("Foo").GetParameters()[0].ParameterType);
            Assert.AreEqual(ParamsPriority.MatchKind.GenericTypeMatch, hoge.matchkind);
            Assert.AreEqual(4, hoge.concreteness);

            hoge = TypeCheck.GetParamsPriority(typeof(List<List<List<int>>>), typeof(List<>));
            Assert.AreEqual(ParamsPriority.MatchKind.GenericTypeMatch, hoge.matchkind);
            Assert.AreEqual(2, hoge.concreteness);

            //GetGenericUpCast
            var hoge2 = TypeCheck.GetGenericUpCastInfo(typeof(char[]), typeof(IEnumerable<>));
            Assert.AreEqual(typeof(IEnumerable<char>), hoge2.Value.upCastedType);

        }

    }
}
