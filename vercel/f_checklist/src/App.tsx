import React, { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Checklist, AppMode } from './types';
import { saveCheckList, loadCheckList } from './storage';
import ChecklistEditor from './components/ChecklistEditor';
import ChecklistRunner from './components/ChecklistRunner';

const checkListTitle = "Fred CheckList";
const checkListId = "123456";
const dbCacheTime1Hour = 1000 * 60 * 60 * 1; // 1 hour

const App: React.FC = () => {

  const [mode, setMode] = useState<AppMode>('edit');
  //const [checklist, setChecklist] = useState<Checklist>({ id: checkListId, title: checkListTitle, items: [] });
  const [checklist, setChecklist] = useState<Checklist>();

  const { data: checkList, isError, isLoading } = useQuery<Checklist, Error>({
      queryKey: ['checkListWebApiQuery'],
      queryFn: async () => await loadCheckList(),
      staleTime: dbCacheTime1Hour,
      gcTime: dbCacheTime1Hour,
  });
  
  useEffect(() => { // Auto-save checklist when it changes
    if(checklist) {
      saveCheckList(checklist);
    }
  }, [checklist]);

  const checkList2 = checklist || checkList;

  if (isLoading) return <LoadingComponent />;
  if (isError) return <ErrorDisplay />;

  if(!checkList2) {
    return (<div>Loading checklist...</div>);
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto px-4 py-8 max-w-4xl">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-4">Checklist App</h1>

          {/* Mode Toggle */}
          <div className="flex gap-2">
            <button onClick={() => setMode('edit')} className={`px-4 py-2 rounded-lg font-medium transition-colors ${mode === 'edit' ? 'bg-blue-600 text-white' : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'}`} >
              Edit Mode
            </button>
            <button onClick={() => setMode('run')} className={`px-4 py-2 rounded-lg font-medium transition-colors ${mode === 'run' ? 'bg-green-600 text-white' : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'}`}>
              Run Mode
            </button>
          </div>
        </div>

        {/* Content */}
        {mode === 'edit' ? (
          <ChecklistEditor checklist={checkList2}  onChecklistChange={setChecklist}  />
        ) : (
          <ChecklistRunner  checklist={checkList2}  onChecklistChange={setChecklist}  />
        )}
      </div>
    </div>
  );
};

const LoadingComponent = () => (
  <div className="flex items-center justify-center min-h-screen bg-gray-50">
    <div className="text-xl font-semibold text-gray-700">Loading...</div>
  </div>
);

const ErrorDisplay = () => (
  <div className="flex items-center justify-center min-h-screen bg-gray-50">
    <div className="text-xl font-semibold text-gray-700">Error</div>
  </div>
);

export default App;