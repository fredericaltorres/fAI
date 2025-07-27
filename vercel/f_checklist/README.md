##################

# Checklist App

A React TypeScript single-page application with Tailwind CSS for creating and managing checklists.

## Features

- **Two Modes**: 
  - **Edit Mode**: Create, modify, and delete checklist items
  - **Run Mode**: Check off completed items and track progress

- **Checklist Items**:
  - Title (required)
  - Optional image from URL
  - Completion status tracking

- **Data Persistence**: 
  - In-memory storage with `saveCheckList()` and `loadCheckList()` methods
  - Auto-save functionality

- **Beautiful UI**:
  - Modern design with Tailwind CSS
  - Progress tracking with visual indicators
  - Responsive layout
  - Completion celebration when all items are done

## Getting Started

### Prerequisites

- Node.js (version 14 or higher)
- npm

### Installation

1. Clone or download this repository
2. Install dependencies:
   ```bash
   npm install
   ```

### Development

Start the development server:
```bash
npm run dev
```

The application will open in your browser at `http://localhost:5173`

### Build for Production

```bash
npm run build
```

## Usage

### Edit Mode
- Change the checklist title
- Add new items with title and optional image URL
- Edit existing item titles and images
- Remove items from the checklist
- Preview images before saving

### Run Mode
- View your checklist with a clean, focused interface
- Check off completed items
- Track progress with visual progress bar
- See completion statistics
- Get celebration when all items are completed

## Project Structure

```
src/
├── components/
│   ├── ChecklistEditor.tsx    # Edit mode interface
│   └── ChecklistRunner.tsx    # Run mode interface
├── types.ts                   # TypeScript type definitions
├── storage.ts                 # In-memory storage functions
├── App.tsx                    # Main application component
├── main.tsx                   # Application entry point
└── index.css                  # Tailwind CSS imports
```

## Technologies Used

- **React 18** - UI framework
- **TypeScript** - Type safety
- **Tailwind CSS** - Styling
- **Vite** - Build tool and development server
- **ESLint** - Code linting

## Storage

The application uses in-memory storage with two main functions:

- `saveCheckList(checklist)` - Saves the current checklist to memory
- `loadCheckList()` - Loads the checklist from memory

Data persists during the current session but will be lost when the browser is refreshed or closed.

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request