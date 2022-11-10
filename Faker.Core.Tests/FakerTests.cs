using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Faker.Core;
using System.Collections.Generic;
using StringGenerator;
using DateTimeGenerator;
using Test;
namespace Faker.Core.Tests;

[TestClass]
public class FakerTest
{
    static public FakerService faker;
    private class TestAllType
    {
        public int Int;
        public float Float;
        public long Long;
        public double Double;
        public char Char;
        public bool Bool;
        public string String;
        public DateTime DateTime;
    }
    private class TestConfig
    {
        public string ConfigString;
        public int ConfigInt;
        public string String;
        public int Int;
        public int PropInt { get; }
        public int PropIntConfig { get; }

        public TestConfig(int PropInt, int PropIntConfig)
        {
            this.PropInt = PropInt;
            this.PropIntConfig = PropIntConfig;
        }
    }
    private class TestConstructor
    {
        public int Int { get; }
        public string String { get; }
        public bool Bool { get; }
        public TestConstructor(int i)
        {

        }
        public TestConstructor(int i, int j)
        {
            Int = -10;
            Bool = false;
            String = "done";
        }
    }
    [TestInitialize]
    public void TestInit()
    {
        FakerConfiguration configuration = new FakerConfiguration();
        configuration.Add<TestConfig, string, NewStringGenerator>(TestConfig => TestConfig.ConfigString);
        configuration.Add<TestConfig, int, NewIntGenerator>(TestConfig => TestConfig.ConfigInt);
        configuration.Add<TestConfig, int, NewIntGenerator>(TestConfig => TestConfig.PropIntConfig);
        faker = new FakerService(configuration);
    }

    [TestMethod]
    public void TestAllTypeMeth()
    {
        TestAllType testAllType = faker.Create<TestAllType>();
        Assert.IsNotNull(testAllType);
        Assert.AreNotEqual(testAllType.Char, "");
    }

    [TestMethod]
    public void ConfigTest()
    {
        string[] RickAstley = { "Never gonna give you up", "Never gonna let you down", "Never gonna run around and desert you", "Never gonna make you cry", "Never gonna say goodbye", "Never gonna tell a lie and hurt you" };
        TestConfig testConfig = faker.Create<TestConfig>();
        Assert.IsNotNull(testConfig);
        Assert.AreEqual(testConfig.ConfigInt, 322);
        Assert.AreEqual(testConfig.PropIntConfig, 322);
        CollectionAssert.Contains(RickAstley, testConfig.ConfigString);

    }
    [TestMethod]
    public void ConstructorTest()
    {
        TestConstructor testConstructor = faker.Create<TestConstructor>();
        Assert.IsNotNull(testConstructor);
        Assert.AreEqual(testConstructor.Int, -10);
        Assert.AreEqual(testConstructor.String, "done");
        Assert.AreEqual(testConstructor.Bool, false);

    }

    [TestMethod]
    public void ListTest()
    {
        List<TestAllType> testConstructor = faker.Create<List<TestAllType>>();
        Assert.IsNotNull(testConstructor);
        CollectionAssert.AllItemsAreNotNull(testConstructor);
    }

    [TestMethod]
    public void DoubleListTest()
    {
        List<List<TestAllType>> testConstructor = faker.Create<List<List<TestAllType>>>();
        Assert.IsNotNull(testConstructor);
        CollectionAssert.AllItemsAreNotNull(testConstructor);
    }
}
