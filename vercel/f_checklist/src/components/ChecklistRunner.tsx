import React from 'react';
import { Checklist, ChecklistItem } from '../types';

interface ChecklistRunnerProps {
  checklist: Checklist;
  onChecklistChange: (checklist: Checklist) => void;
}

const ChecklistRunner: React.FC<ChecklistRunnerProps> = ({ checklist, onChecklistChange }) => {
  const toggleItemCompletion = (itemId: string) => {
    const updatedChecklist = {
      ...checklist,
      items: checklist.items.map(item =>
        item.id === itemId ? { ...item, completed: !item.completed } : item
      )
    };
    onChecklistChange(updatedChecklist);
  };

  const completedCount = checklist.items.filter(item => item.completed).length;
  const totalCount = checklist.items.length;
  const progressPercentage = totalCount > 0 ? (completedCount / totalCount) * 100 : 0;

  return (
    <div className="space-y-6">
      {/* Checklist Header */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h1 className="text-2xl font-bold text-gray-900 mb-4">{checklist.title}</h1>
        
        {/* Progress Bar */}
        <div className="mb-4">
          <div className="flex justify-between items-center mb-2">
            <span className="text-sm font-medium text-gray-700">Progress</span>
            <span className="text-sm text-gray-600">
              {completedCount} of {totalCount} completed
            </span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div
              className="bg-green-600 h-2 rounded-full transition-all duration-300"
              style={{ width: `${progressPercentage}%` }}
            ></div>
          </div>
        </div>

        {/* Progress Stats */}
        <div className="grid grid-cols-3 gap-4 text-center">
          <div className="bg-gray-50 rounded-lg p-3">
            <div className="text-2xl font-bold text-gray-900">{totalCount}</div>
            <div className="text-sm text-gray-600">Total Items</div>
          </div>
          <div className="bg-green-50 rounded-lg p-3">
            <div className="text-2xl font-bold text-green-600">{completedCount}</div>
            <div className="text-sm text-gray-600">Completed</div>
          </div>
          <div className="bg-blue-50 rounded-lg p-3">
            <div className="text-2xl font-bold text-blue-600">{totalCount - completedCount}</div>
            <div className="text-sm text-gray-600">Remaining</div>
          </div>
        </div>
      </div>

      {/* Checklist Items */}
      <div className="bg-white rounded-lg shadow-md p-6">
        {checklist.items.length === 0 ? (
          <div className="text-center py-12">
            <div className="text-gray-400 mb-4">
              <svg className="mx-auto h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
              </svg>
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">No items in checklist</h3>
            <p className="text-gray-600">Switch to Edit mode to add items to your checklist.</p>
          </div>
        ) : (
          <div className="space-y-4">
            {checklist.items.map((item, index) => (
              <div
                key={item.id}
                className={`flex items-start space-x-4 p-4 rounded-lg border-2 transition-all ${
                  item.completed
                    ? 'bg-green-50 border-green-200'
                    : 'bg-gray-50 border-gray-200 hover:border-gray-300'
                }`}
              >
                {/* Checkbox */}
                <div className="flex-shrink-0 pt-1">
                  <button
                    onClick={() => toggleItemCompletion(item.id)}
                    className={`w-6 h-6 rounded-md border-2 flex items-center justify-center transition-colors ${
                      item.completed
                        ? 'bg-green-600 border-green-600 text-white'
                        : 'border-gray-300 hover:border-gray-400'
                    }`}
                  >
                    {item.completed && (
                      <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                        <path
                          fillRule="evenodd"
                          d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                          clipRule="evenodd"
                        />
                      </svg>
                    )}
                  </button>
                </div>

                {/* Content */}
                <div className="flex-grow min-w-0">
                  <div className="flex items-start space-x-4">
                    {/* Image */}
                    {item.imageUrl && (
                      <div className="flex-shrink-0">
                        <img
                          src={item.imageUrl}
                          alt={item.title}
                          className="w-16 h-16 object-cover rounded-lg border border-gray-300"
                          onError={(e) => {
                            e.currentTarget.style.display = 'none';
                          }}
                        />
                      </div>
                    )}

                    {/* Text Content */}
                    <div className="flex-grow min-w-0">
                      <div className="flex items-center space-x-2 mb-1">
                        <span className="text-sm font-medium text-gray-500">
                          #{index + 1}
                        </span>
                      </div>
                      <h3
                        className={`text-lg font-medium transition-all ${
                          item.completed
                            ? 'text-green-800 line-through'
                            : 'text-gray-900'
                        }`}
                      >
                        {item.title}
                      </h3>
                    </div>
                  </div>
                </div>

                {/* Status Badge */}
                <div className="flex-shrink-0">
                  <span
                    className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      item.completed
                        ? 'bg-green-100 text-green-800'
                        : 'bg-gray-100 text-gray-800'
                    }`}
                  >
                    {item.completed ? 'Completed' : 'Waiting'}
                  </span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Completion Celebration */}
      {totalCount > 0 && completedCount === totalCount && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-6 text-center">
          <div className="text-green-600 mb-4">
            <svg className="mx-auto h-16 w-16" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <h3 className="text-xl font-bold text-green-800 mb-2">
            🎉 Congratulations! 🎉
          </h3>
          <p className="text-green-700">
            You've completed all items in your checklist!
          </p>
        </div>
      )}
    </div>
  );
};

export default ChecklistRunner;