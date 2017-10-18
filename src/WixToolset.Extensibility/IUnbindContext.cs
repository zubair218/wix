﻿// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Extensibility
{
    using WixToolset.Data;

    public interface IUnbindContext
    {
        string ExportBasePath { get; set; }

        string InputFilePath { get; set; }

        string IntermediateFolder { get; set; }

        bool IsAdminImage { get; set; }

        Messaging Messaging { get; }

        bool SuppressDemodularization { get; set; }

        bool SuppressExtractCabinets { get; set; }
    }
}