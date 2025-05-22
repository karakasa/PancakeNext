namespace SvgIcon.UnitTests;

public class Tests
{
    const string Name1 = "testFile1";
    const string Name2 = "testFile2";

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void SimpleRoundtrip()
    {
        var rng = new Random();
        var buffer1 = new byte[4096];
        var buffer2 = new byte[1024];
        rng.NextBytes(buffer1);
        rng.NextBytes(buffer2);

        var archive = new ByteArchive();
        archive.Add(Name1, buffer1);
        archive.Add(Name2, buffer2);

        using (var fs = File.Open(@"E:\Gan\test.archive", FileMode.Create))
        {
            archive.WriteTo(fs);
        }

        using (var fs = File.Open(@"E:\Gan\test.archive", FileMode.Open))
        {
            archive = ByteArchive.CreateFrom(fs);
        }

        Assert.That(archive.TryGet(Name1, out var bytes1), Is.True);
        Assert.That(archive.TryGet(Name2, out var bytes2), Is.True);

        Assert.That(bytes1, Is.EqualTo(buffer1));
        Assert.That(bytes2, Is.EqualTo(buffer2));

        Assert.That(bytes1, Is.Not.SameAs(buffer1));
        Assert.That(bytes2, Is.Not.SameAs(buffer2));

        Assert.That(archive.Entries.Select(x => x.Name), Is.EqualTo(new[] { Name1, Name2 }));
    }
}