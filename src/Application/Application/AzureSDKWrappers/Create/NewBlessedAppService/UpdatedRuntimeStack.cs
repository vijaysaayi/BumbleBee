using Microsoft.Azure.Management.AppService.Fluent;

namespace Penguin.Code.Application.AzureSDKWrappers.Create.NewBlessedAppService
{
    public class UpdatedRuntimeStack : RuntimeStack
    {
        public UpdatedRuntimeStack(string stack, string version)
            : base(stack, version)
        {

        }

        public static readonly RuntimeStack Python_3_8 = new RuntimeStack("PYTHON", "3.8");
    }
}