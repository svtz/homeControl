﻿using System;

namespace homeControl.WebApi.Server
{
    public interface IClientListener : IDisposable
    {
        void StartListening();

        /// <remarks>this does not disconnect clients already connected</remarks>
        void StopListening();
    }
}