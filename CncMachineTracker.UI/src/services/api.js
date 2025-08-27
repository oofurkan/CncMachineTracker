const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

class ApiService {
  async getMachines() {
    try {
      const response = await fetch(`${API_BASE_URL}/machines`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Error fetching machines:', error);
      throw error;
    }
  }

  async getMachineById(id) {
    try {
      const response = await fetch(`${API_BASE_URL}/machines/${id}`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error(`Error fetching machine ${id}:`, error);
      throw error;
    }
  }

  async simulateMachine() {
    try {
      const response = await fetch(`${API_BASE_URL}/machines/simulate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Error simulating machine:', error);
      throw error;
    }
  }
}

export default new ApiService();
