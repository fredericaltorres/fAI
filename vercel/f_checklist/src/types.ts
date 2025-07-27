export interface ChecklistItem {
  id: string;
  title: string;
  imageUrl?: string;
  completed: boolean;
}

export interface Checklist {
  id: string;
  title: string;
  items: ChecklistItem[];
}

export type AppMode = 'edit' | 'run';