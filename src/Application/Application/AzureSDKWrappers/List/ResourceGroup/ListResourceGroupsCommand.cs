﻿using MediatR;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System.Collections.Generic;

namespace Penguin.Code.Application.AzureSDKWrappers.List.ResourceGroup
{
    public class ListResourceGroupsCommand : IRequest<List<string>>
    {
    }
}