using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using XamlingCore.Portable.Contract.Entities;
using XamlingCore.Tests.Droid.Base;

namespace XamlingCore.Droid.Tests.CacheTests
{
    [TestFixture]
    public class CacheTests : TestBase
    {

        public class MyObject
        {
            public string ItemName { get; set; }
        }

        public class MyOtherObject
        {
            public string ItemName { get; set; }
        }
        [Test]
        public void Test_Cache_Clear_All()
        {
            var cache = Container.Resolve<IEntityCache>();

            var msr = new ManualResetEvent(false);

            Task.Run(async () =>
            {

                var item1 = new MyObject { ItemName = "Item1" };
                var item2 = new MyObject { ItemName = "Item2" };

                var item1a = new MyOtherObject { ItemName = "Item1" };
                var item2a = new MyOtherObject { ItemName = "Item2" };


                await cache.SetEntity(item1.ItemName, item1);
                await cache.SetEntity(item2.ItemName, item2);
                await cache.SetEntity(item1a.ItemName, item1a);
                await cache.SetEntity(item2a.ItemName, item2a);

                var result1 = await cache.GetEntity<MyObject>(item1.ItemName);
                var result2 = await cache.GetEntity<MyObject>(item2.ItemName);
                var result3 = await cache.GetEntity<MyOtherObject>(item1a.ItemName);
                var result4 = await cache.GetEntity<MyOtherObject>(item2a.ItemName);

                Assert.IsNotNull(result1);
                Assert.IsNotNull(result2);
                Assert.IsNotNull(result3);
                Assert.IsNotNull(result4);

                await cache.Clear();

                var result1_e = await cache.GetEntity<MyObject>(item1.ItemName);
                var result2_e = await cache.GetEntity<MyObject>(item2.ItemName);
                var result3_e = await cache.GetEntity<MyOtherObject>(item1a.ItemName);
                var result4_e = await cache.GetEntity<MyOtherObject>(item2a.ItemName);

                Assert.IsNull(result1_e);
                Assert.IsNull(result2_e);
                Assert.IsNull(result3_e);
                Assert.IsNull(result4_e);


                msr.Set();
            });

            var msrResult = msr.WaitOne(20000);
            Assert.IsTrue(msrResult, "MSR not set, means assertion failed in task");
        }

        [Test]
        public void Test_Clear_And_Read_All()
        {
            var cache = Container.Resolve<IEntityCache>();

            var msr = new ManualResetEvent(false);

            Task.Run(async () =>
            {
                var item1 = new MyObject {ItemName = "Item1"};
                var item2 = new MyObject { ItemName = "Item2" };
                var item3 = new MyObject { ItemName = "Item3" };
                var item4 = new MyObject { ItemName = "Item4" };

                await cache.SetEntity(item1.ItemName, item1);

                var result = await cache.GetEntity<MyObject>(item1.ItemName);

                Assert.IsNotNull(result);

                await cache.DeleteAll<MyObject>();

                var result2 = await cache.GetEntity<MyObject>(item1.ItemName);
                Assert.IsNull(result2);


                await cache.SetEntity(item1.ItemName, item1);
                await cache.SetEntity(item2.ItemName, item2);
                await cache.SetEntity(item3.ItemName, item3);
                await cache.SetEntity(item4.ItemName, item4);



                var result3 = await cache.GetEntity<MyObject>(item1.ItemName);
                var result4 = await cache.GetEntity<MyObject>(item2.ItemName);
                var result5 = await cache.GetEntity<MyObject>(item3.ItemName);
                var result6 = await cache.GetEntity<MyObject>(item4.ItemName);

                Assert.IsNotNull(result3);
                Assert.IsNotNull(result4);
                Assert.IsNotNull(result5);
                Assert.IsNotNull(result6);


                var all = await cache.GetAll<MyObject>();
                Assert.IsFalse(all.Count == 0);

                await cache.Delete<MyObject>(item2.ItemName);

                var result7 = await cache.GetEntity<MyObject>(item1.ItemName);
                var result8 = await cache.GetEntity<MyObject>(item2.ItemName);
                var result9 = await cache.GetEntity<MyObject>(item3.ItemName);
                var result10 = await cache.GetEntity<MyObject>(item4.ItemName);
                var result10a = await cache.GetEntity<MyObject>("Some other thing");

                Assert.IsNotNull(result7);
                Assert.IsNull(result8);
                Assert.IsNull(result10a);
                Assert.IsNotNull(result9);
                Assert.IsNotNull(result10);

                await cache.DeleteAll<MyObject>();
                var result11 = await cache.GetEntity<MyObject>(item1.ItemName);
                var result12 = await cache.GetEntity<MyObject>(item2.ItemName);
                var result13 = await cache.GetEntity<MyObject>(item3.ItemName);
                var result14 = await cache.GetEntity<MyObject>(item4.ItemName);
                Assert.IsNull(result11);
                Assert.IsNull(result12);
                Assert.IsNull(result13);
                Assert.IsNull(result14);

                msr.Set();
            });

            var msrResult = msr.WaitOne(20000);
            Assert.IsTrue(msrResult, "MSR not set, means assertion failed in task");
        }

        [Test]
        public void Check_Parallel_Test()
        {
            var cache = Container.Resolve<IEntityCache>();

            var msr = new ManualResetEvent(false);

            Task.Run(async () =>
            {
                var list = new List<string>();

                await cache.SetEntity("EmptyList", list);

                var a = cache.GetEntity<List<string>>("EmptyList", _serverGetListGuid, null, true, false);
                var b = cache.GetEntity<List<string>>("EmptyList", _serverGetListGuid, null, true, false);
                var cc = cache.GetEntity<List<string>>("EmptyList", _serverGetListGuid, null, true, false);
                var d = cache.GetEntity<List<string>>("EmptyList", _serverGetListGuid, null, true, false);
                var e = cache.GetEntity<List<string>>("EmptyList", _serverGetListGuid, null, true, false);
                var f = cache.GetEntity<List<string>>("EmptyList", _serverGetListGuid, null, true, false);

                var result = await Task.WhenAll(a, b, cc, d, e, f);

                var firsResult = result.FirstOrDefault();

                Assert.IsNotNull(firsResult);

                Assert.IsTrue(firsResult.Count != 0);

                var firstGuid = firsResult.FirstOrDefault();
                
                foreach (var item in result)
                {
                    Assert.AreEqual(item.FirstOrDefault(), firstGuid);
                }
                msr.Set();
            });

            var msrResult = msr.WaitOne(3000);
            Assert.IsTrue(msrResult, "MSR not set, means assertion failed in task");

        }
        async Task<List<string>> _serverGetListGuid()
        {
            await Task.Delay(200);
            return new List<string>()
            {
                Guid.NewGuid().ToString()
            };
        }
        [Test]
        public void Check_Zero_Lists()
        {
            var cache = Container.Resolve<IEntityCache>();

            var msr = new ManualResetEvent(false);

            Task.Run(async () =>
            {
                var list = new List<string>();

                await cache.SetEntity("EmptyList", list);

                var listGet = await cache.GetEntity<List<string>>("EmptyList", _serverGetList, null, true, true);

                Assert.IsTrue(listGet.Count == 0);

                var listGetFromServer = await cache.GetEntity<List<string>>("EmptyList", _serverGetList, null, true, false);
                Assert.IsTrue(listGetFromServer.Count != 0);
                Assert.AreEqual(listGetFromServer.FirstOrDefault(), "Jordan");
                msr.Set();
            });

            var msrResult = msr.WaitOne(3000);
            Assert.IsTrue(msrResult, "MSR not set, means assertion failed in task");
        }

        async Task<List<string>> _serverGetList()
        {
            return new List<string>()
            {
                "Jordan"
            };
        }
        [Test]
        public void Cache_Get_AWAITER()
        {
            var cache = Container.Resolve<IEntityCache>();

            var msr = new ManualResetEvent(false);

            Task.Run(async () =>
            {
                var cItem = await cache.GetEntity("AwaitedItem", () => GetItem("Awaited"), maxAge: TimeSpan.FromSeconds(5));

                Assert.IsNotNull(cItem);
                Assert.AreEqual(cItem.Field, "Awaited");

                var cItem2 = await cache.GetEntity<CacheTest2>("AwaitedItem", TimeSpan.FromSeconds(5));
                Assert.IsNotNull(cItem2);
                Assert.AreEqual(cItem2.Field, "Awaited");


                var cItem3 = await cache.GetEntity("AwaitedItem", () => GetItem("Awaited2"), maxAge: TimeSpan.FromSeconds(5));

                //the new item will not have done the call back, as it was cached
                Assert.IsNotNull(cItem3);
                Assert.AreEqual(cItem3.Field, "Awaited");

                await cache.Delete<CacheTest2>("AwaitedItem");
                var cItem4 = await cache.GetEntity("AwaitedItem", () => GetItem("Awaited3"), maxAge: TimeSpan.FromSeconds(5));
                Assert.IsNotNull(cItem4);
                Assert.AreEqual(cItem4.Field, "Awaited3");
                msr.Set();
            });

            var msrResult = msr.WaitOne(3000);
            Assert.IsTrue(msrResult, "MSR not set, means assertion failed in task");
        }

        private async Task<CacheTest2> GetItem(string fieldName)
        {
            return new CacheTest2 { Field = fieldName };
        }

        [Test]
        public void Cache_Get_Set()
        {
            var cache = Container.Resolve<IEntityCache>();

            var msr = new ManualResetEvent(false);

            Task.Run(async () =>
            {
                var l1 = new List<CacheTest1>();
                var l2 = new List<CacheTest2>();

                for (var i = 0; i < 15; i++)
                {
                    var c1 = new CacheTest1
                    {
                        Field = "F" + i
                    };

                    l1.Add(c1);

                    await cache.SetEntity(c1.Field, c1);

                    var c2 = new CacheTest2
                    {
                        Field = "F" + i
                    };

                    l2.Add(c2);

                    await cache.SetEntity(c2.Field, c2);
                }

                await cache.SetEntity("List1", l1);
                await cache.SetEntity("List2", l2);

                var c1Out = await cache.GetEntity<List<CacheTest1>>("List1", TimeSpan.FromSeconds(5));
                var c2Out = await cache.GetEntity<List<CacheTest2>>("List2", TimeSpan.FromSeconds(5));

                Assert.IsNotNull(c1Out);
                Assert.IsNotNull(c2Out);

                Assert.AreEqual(c2Out.Count, 15);
                Assert.AreEqual(c1Out.Count, 15);

                await cache.Delete<List<CacheTest1>>("List1");

                var c1Out2 = await cache.GetEntity<List<CacheTest1>>("List1", TimeSpan.FromSeconds(5));
                var c2Out2 = await cache.GetEntity<List<CacheTest2>>("List2", TimeSpan.FromSeconds(5));

                var c1OutMissingBadName = await cache.GetEntity<List<CacheTest1>>("List3", TimeSpan.FromSeconds(5));

                Assert.IsNotNull(c2Out2);
                Assert.IsNull(c1Out2);
                Assert.IsNull(c1OutMissingBadName);

                var c1Out3 = await cache.GetEntity<List<CacheTest2>>("List1", TimeSpan.FromSeconds(5));
                var c2Out3 = await cache.GetEntity<List<CacheTest1>>("List2", TimeSpan.FromSeconds(5));

                Assert.IsNull(c2Out3);
                Assert.IsNull(c1Out3);

                var f1 = await cache.GetEntity<CacheTest1>("F1", TimeSpan.FromSeconds(5));
                var f2 = await cache.GetEntity<CacheTest2>("F1", TimeSpan.FromSeconds(5));

                Assert.IsNotNull(f1);
                Assert.IsNotNull(f2);


                await cache.Delete<CacheTest1>("F1");

                var f12 = await cache.GetEntity<CacheTest1>("F1", TimeSpan.FromSeconds(5));
                var f22 = await cache.GetEntity<CacheTest2>("F1", TimeSpan.FromSeconds(5));

                Assert.IsNull(f12);
                Assert.IsNotNull(f22);

                msr.Set();
            });

            var msrResult = msr.WaitOne(3000);
            Assert.IsTrue(msrResult, "MSR not set, means assertion failed in task");
        }

        [Test]
        public void Cache_Timeout_test()
        {
            var cache = Container.Resolve<IEntityCache>();

            var msr = new ManualResetEvent(false);

            Task.Run(async () =>
            {
                var c2 = new CacheTest2
                {
                    Field = "FJK"
                };

                await cache.SetEntity("FJK", c2);

                var beforeTimeout = await cache.GetEntity<CacheTest2>("FJK", TimeSpan.FromSeconds(1));

                await Task.Delay(2000);
                var afterTimeout = await cache.GetEntity<CacheTest2>("FJK", TimeSpan.FromSeconds(1));

                Assert.IsNotNull(beforeTimeout);
                Assert.IsNull(afterTimeout);
                msr.Set();
            });

            var msrResult = msr.WaitOne(3000);
            Assert.IsTrue(msrResult, "MSR not set, means assertion failed in task");
            
        }

    }

    public class CacheTest1
    {
        public string Field { get; set; }
    }

    public class CacheTest2
    {
        public string Field { get; set; }
    }
}