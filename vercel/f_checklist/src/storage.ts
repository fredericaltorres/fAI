import { Checklist } from './types';

// In-memory storage
let checklistData: Checklist | null = null;

const checkListFileName = "FredCheckList";
const apiUrl = `https://faiwebapi.azurewebsites.net/JsonCloudDB?filename=${checkListFileName}`;

export const saveCheckList = (checklist: Checklist): void => {
  console.log('Saving checklist to server:', JSON.stringify(checklist));
  checklistData = checklist;
  const body = JSON.stringify(checklist);
  fetch(apiUrl, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: body,
  })
  .then(response => {
    if (!response.ok) {
      throw new Error("Failed to save checklist to server");
    }
    return response.json();
  })
  .then(data => {
    console.log('Checklist saved to server:', data);
  })
  .catch(error => {
    console.error('Error saving checklist to server:', error);
  });
};

export const loadCheckList = (): Checklist | null => {
  // Note: fetch is asynchronous, but this function is synchronous.
  // To keep the API the same, we use a workaround: warn and return null.
  // For real async loading, refactor the app to use async/await.
  console.log('Loading checklist from server');
  console.warn('loadCheckList is now asynchronous and should be refactored to return a Promise.');
  let checkList: Checklist | null = null;
  fetch(apiUrl, {
    method: "GET",
    headers: {
      "Content-Type": "application/json"
    }
  })
    .then(response => {
      if (!response.ok) {
        throw new Error(`Failed to load checklist from server: ${response.status} ${response.statusText}`);
      }
      return response.json();
    })
    .then(data => {
      checkList = data as Checklist;
      console.log('Checklist loaded from server:', checkList);
      // Optionally, you could trigger a callback or event here to update the app.
    })
    .catch(error => {
      console.error('Error loading checklist from server:', error);
    });
  return checkList;
};