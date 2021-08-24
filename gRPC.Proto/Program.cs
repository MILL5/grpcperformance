using Performance.Contracts;
using ProtoBuf.Grpc.Reflection;
using ProtoBuf.Meta;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace gRPC.Proto
{
    class Program
    {
        static async Task Main()
        {
            var generator = new SchemaGenerator
            {
                ProtoSyntax = ProtoSyntax.Proto3
            };

            var schema = generator.GetSchema<ISampleService>(); // there is also a non-generic overload that takes Type

            using (var writer = new System.IO.StreamWriter("services.proto"))
            {
                await writer.WriteAsync(schema);
            }

            var filename = new FileInfo("services.proto").FullName;

            using Process process = Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
        }
    }
}
