﻿using MediatR;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Rest.Azure;

namespace Penguin.Code.Application.AzureSDKWrappers.Update.ConnectionStrings
{
    public class UpdateConnectionStringsCommand : IRequest<AzureOperationResponse<ConnectionStringDictionaryInner>>
    {
        public string ResourceGroup { get; set; }

        public string AppServiceName { get; set; }

        public ConnectionStringDictionaryInner ConnectionStrings { get; set; }
    }
}