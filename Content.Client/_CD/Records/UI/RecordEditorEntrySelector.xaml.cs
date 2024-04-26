using System.Linq;
using Content.Shared._CD.Records;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._CD.Records.UI;

/// <summary>
/// The box that contains the list of entities in the record editor. We create one for each record type
/// </summary>
[GenerateTypedNameReferences]
public sealed partial class RecordEditorEntrySelector : Control
{
    private List<CharacterRecords.RecordEntry> _entries = new();

    public event Action<RecordEditorEntryUpdateArgs>? OnUpdateEntries;

    private readonly RecordEntryEditPopup _editPopup = new();
    private readonly RecordEntryViewPopup _entryViewPopup = new();
    private int _editIdx;

    public RecordEditorEntrySelector()
    {
        RobustXamlLoader.Load(this);

        AddButton.OnPressed += _ =>
        {
            _editIdx = _entries.Count;
            _editPopup.SetContents(new CharacterRecords.RecordEntry("", "", ""));
            _editPopup.Open();
        };

        EditButton.OnPressed += _ =>
        {
            if (!EntrySelector.GetSelected().Any())
                return;
            _editIdx = EntrySelector.IndexOf(EntrySelector.GetSelected().First());
            _editPopup.SetContents(_entries[_editIdx]);
            _editPopup.Open();
        };

        ViewButton.OnPressed += _ =>
        {
            if (!EntrySelector.GetSelected().Any())
                return;
            var idx = EntrySelector.IndexOf(EntrySelector.GetSelected().First());
            _entryViewPopup.SetContents(_entries[idx]);
            _entryViewPopup.Open();
        };

        RemoveButton.OnPressed += _ =>
        {
            if (!EntrySelector.GetSelected().Any())
                return;
            // Remove the entry, being careful to set the index correctly
            int idx = EntrySelector.IndexOf(EntrySelector.GetSelected().First());
            EntrySelector.RemoveAt(idx);
            _entries.RemoveAt(idx);
            if (idx == _editIdx)
                _editPopup.Close();
            _editIdx--;
            OnUpdateEntries?.Invoke(new RecordEditorEntryUpdateArgs(_entries));
        };

        UpButton.OnPressed += _ =>
        {
            if (!EntrySelector.GetSelected().Any())
                return;
            int idx = EntrySelector.IndexOf(EntrySelector.GetSelected().First());
            if (idx < 1)
                return;
            (_entries[idx], _entries[idx - 1]) = (_entries[idx - 1], _entries[idx]);
            (EntrySelector[idx], EntrySelector[idx - 1]) = (EntrySelector[idx - 1], EntrySelector[idx]);
            OnUpdateEntries?.Invoke(new RecordEditorEntryUpdateArgs(_entries));
        };

        DownButton.OnPressed += _ =>
        {
            if (!EntrySelector.GetSelected().Any())
                return;
            int idx = EntrySelector.IndexOf(EntrySelector.GetSelected().First());
            if (idx >= EntrySelector.Count - 1)
                return;
            (_entries[idx], _entries[idx + 1]) = (_entries[idx + 1], _entries[idx]);
            (EntrySelector[idx], EntrySelector[idx + 1]) = (EntrySelector[idx + 1], EntrySelector[idx]);
            OnUpdateEntries?.Invoke(new RecordEditorEntryUpdateArgs(_entries));
        };

        _editPopup.SaveButton.OnPressed += _ =>
        {
            if (_editIdx >= _entries.Count)
            {
                var rec = _editPopup.GetContents();
                _entries.Add(rec);
                EntrySelector.AddItem(rec.Title);
            }
            else
            {
                _entries[_editIdx] = _editPopup.GetContents();
                EntrySelector[_editIdx].Text = _entries[_editIdx].Title;
            }
            OnUpdateEntries?.Invoke(new RecordEditorEntryUpdateArgs(_entries));
        };
        OnVisibilityChanged += _ =>
        {
            _editPopup.Close();
        };
    }

    public void UpdateContents(List<CharacterRecords.RecordEntry> entries)
    {
        _entries = entries;
        RefreshSelector();
    }

    private void RefreshSelector()
    {
        EntrySelector.Clear();
        foreach (var entry in _entries)
        {
            EntrySelector.AddItem(entry.Title);
        }
    }

    public sealed class RecordEditorEntryUpdateArgs
    {
        public List<CharacterRecords.RecordEntry> Entries { get; private set; }

        public RecordEditorEntryUpdateArgs(List<CharacterRecords.RecordEntry> entries)
        {
            Entries = entries;
        }
    }
}
