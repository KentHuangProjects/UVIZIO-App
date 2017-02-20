﻿using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public abstract class DescriptorBase : IDescriptor
    {
        private string _name;

        public abstract Guid Id { get; }

        public string Name => _name ?? (_name = KnownDescriptors.Lookup(Id).Name);

        public abstract byte[] Value { get; }

        public Task<byte[]> ReadAsync()
        {
            return ReadNativeAsync();
        }

        protected abstract Task<byte[]> ReadNativeAsync();

        public Task WriteAsync(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return WriteNativeAsync(data);
        }

        protected abstract Task WriteNativeAsync(byte[] data);
       
    }
}