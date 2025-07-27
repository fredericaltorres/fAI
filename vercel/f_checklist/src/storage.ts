import { Checklist } from './types';


const trace = (message: string) => console.log(message);

// In-memory storage

const checkListFileName = "FredCheckList";
const apiUrl = `https://faiwebapi.azurewebsites.net/JsonCloudDB?filename=${checkListFileName}`;

export const saveCheckList = (checklist: Checklist): void => {
  console.log('Saving checklist to server:', JSON.stringify(checklist));
  const body = JSON.stringify(checklist);
  fetch(apiUrl, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body,
  })
  .then(response => {
    if (!response.ok) 
      throw new Error("Failed to save checklist to server");
    return response.json();
  })
  .then(data => {
    console.log('Checklist saved to server:', data);
  })
  .catch(error => {
    console.error('Error saving checklist to server:', error);
  });
};

export const loadCheckList = async (): Promise<Checklist> => {
  try {

    const startTime = performance.now();
    const response = await fetch(apiUrl);
    if (!response.ok)  throw new Error(`HTTP error! status: ${response.status}`);
    const data = await response.json();
    const checklist = data as Checklist;
    const endTime = performance.now();
    const duration = Math.round((endTime - startTime) / 1000 * 100) / 100;
    trace(`Fetched ${checklist.items.length} items, Client duration: ${duration} seconds`);
    return checklist;
  }
  catch (error) {
    console.error('Error fetching JSON data:', error);
    throw error;
  }
};
