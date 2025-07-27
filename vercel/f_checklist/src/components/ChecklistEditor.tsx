import React, { useState } from 'react';
import { Checklist, ChecklistItem } from '../types';

interface ChecklistEditorProps {
  checklist: Checklist;
  onChecklistChange: (checklist: Checklist) => void;
}

const ChecklistEditor: React.FC<ChecklistEditorProps> = ({ checklist, onChecklistChange }) => {
  const [newItemTitle, setNewItemTitle] = useState('');
  const [newItemImageUrl, setNewItemImageUrl] = useState('');

  const addItem = () => {
    if (newItemTitle.trim()) {
      const newItem: ChecklistItem = {
        id: Date.now().toString(),
        title: newItemTitle.trim(),
        imageUrl: newItemImageUrl.trim() || undefined,
        completed: false
      };

      const updatedChecklist = {
        ...checklist,
        items: [...checklist.items, newItem]
      };

      onChecklistChange(updatedChecklist);
      setNewItemTitle('');
      setNewItemImageUrl('');
    }
  };

  const removeItem = (itemId: string) => {
    const updatedChecklist = {
      ...checklist,
      items: checklist.items.filter(item => item.id !== itemId)
    };
    onChecklistChange(updatedChecklist);
  };

  const updateItem = (itemId: string, updates: Partial<ChecklistItem>) => {
    const updatedChecklist = {
      ...checklist,
      items: checklist.items.map(item =>
        item.id === itemId ? { ...item, ...updates } : item
      )
    };
    onChecklistChange(updatedChecklist);
  };

  const updateChecklistTitle = (title: string) => {
    onChecklistChange({ ...checklist, title });
  };

  return (
    <div className="space-y-6">
      {/* Checklist Title Editor */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Checklist Title
        </label>
        <input
          type="text"
          value={checklist.title}
          onChange={(e) => updateChecklistTitle(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Enter checklist title..."
        />
      </div>

      {/* Add New Item */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Add New Item</h2>
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Title *
            </label>
            <input
              type="text"
              value={newItemTitle}
              onChange={(e) => setNewItemTitle(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Enter item title..."
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Image URL (optional)
            </label>
            <input
              type="url"
              value={newItemImageUrl}
              onChange={(e) => setNewItemImageUrl(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Enter image URL..."
            />
          </div>
          <button
            onClick={addItem}
            disabled={!newItemTitle.trim()}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            Add Item
          </button>
        </div>
      </div>

      {/* Existing Items */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">
          Items ({checklist.items.length})
        </h2>
        {checklist.items.length === 0 ? (
          <p className="text-gray-500 text-center py-8">
            No items added yet. Create your first item above!
          </p>
        ) : (
          <div className="space-y-4">
            {checklist.items.map((item) => (
              <div key={item.id} className="border border-gray-200 rounded-lg p-4">
                <div className="space-y-3">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Title
                    </label>
                    <input
                      type="text"
                      value={item.title}
                      onChange={(e) => updateItem(item.id, { title: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Image URL
                    </label>
                    <input
                      type="url"
                      value={item.imageUrl || ''}
                      onChange={(e) => updateItem(item.id, { imageUrl: e.target.value || undefined })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                  {item.imageUrl && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Preview
                      </label>
                      <img
                        src={item.imageUrl}
                        alt={item.title}
                        className="w-24 h-24 object-cover rounded-md border border-gray-300"
                        onError={(e) => {
                          e.currentTarget.style.display = 'none';
                        }}
                      />
                    </div>
                  )}
                  <div className="flex justify-end">
                    <button
                      onClick={() => removeItem(item.id)}
                      className="bg-red-600 text-white px-3 py-1 rounded-md hover:bg-red-700 transition-colors text-sm"
                    >
                      Remove
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default ChecklistEditor;