using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using translation.Models;
namespace translation.Services
{
    public class NoteService
    {
        public static event Action OnNoteChanged;
        private readonly string _notesFilePath;
        public NoteService()
        {
            var databaseDir = Path.Combine(AppContext.BaseDirectory, "database");
            if (!Directory.Exists(databaseDir))
            {
                Directory.CreateDirectory(databaseDir);
            }
            _notesFilePath = Path.Combine(databaseDir, "notes.json");
        }
        public async Task<List<NoteItem>> GetAllNotesAsync()
        {
            if (!File.Exists(_notesFilePath)) return new List<NoteItem>();
            try
            {
                var json = await File.ReadAllTextAsync(_notesFilePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<NoteItem>>(json, options) ?? new List<NoteItem>();
            }
            catch
            {
                return new List<NoteItem>();
            }
        }
        public async Task SaveNoteAsync(NoteItem note)
        {
            var notes = await GetAllNotesAsync();
            notes.Insert(0, note); 
            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var json = JsonSerializer.Serialize(notes, options);
            await File.WriteAllTextAsync(_notesFilePath, json);
            OnNoteChanged?.Invoke();
        }
        public async Task UpdateNoteAsync(NoteItem updatedNote)
        {
            var notes = await GetAllNotesAsync();
            var index = notes.FindIndex(n => n.Id == updatedNote.Id);
            if (index >= 0)
            {
                notes[index] = updatedNote;
                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                var json = JsonSerializer.Serialize(notes, options);
                await File.WriteAllTextAsync(_notesFilePath, json);
                OnNoteChanged?.Invoke();
            }
        }
        public async Task DeleteNoteAsync(string noteId)
        {
            var notes = await GetAllNotesAsync();
            var count = notes.RemoveAll(n => n.Id == noteId);
            if (count > 0)
            {
                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                var json = JsonSerializer.Serialize(notes, options);
                await File.WriteAllTextAsync(_notesFilePath, json);
                OnNoteChanged?.Invoke();
            }
        }
    }
}