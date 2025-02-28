using System.Threading;

namespace QuixStreams.RawReadSamples
{
    class Program
    {
        static void Main(string[] args)
        {

            (new Thread(() =>
            {
                TestReadMeta.Run();
            })).Start();

            (new Thread(() =>
            {
                TestWriteMeta.Run();
            })).Start();


/*
            (new Thread(() =>
            {
                TestReadKey.Run();
            })).Start();

            (new Thread(() =>
            {
                TestWriteKey.Run();
            })).Start();
*/
        }
    }
}
