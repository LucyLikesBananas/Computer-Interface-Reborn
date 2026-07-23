using System;

namespace ComputerInterface.Interfaces;

public interface IComputerModEntry {
        string EntryName { get; }

        Type EntryViewType { get; }
}