const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5217/api';

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

  async getMachineHistory(id, minutes = 10) {
    try {
      const response = await fetch(`${API_BASE_URL}/machines/${id}/history?minutes=${minutes}`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error(`Error fetching machine ${id} history:`, error);
      throw error;
    }
  }

  async simulateMachine(id) {
    try {
      const response = await fetch(`${API_BASE_URL}/machines/${id}/simulate`, {
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
      console.error(`Error simulating machine ${id}:`, error);
      throw error;
    }
  }

  async refreshMachine(id) {
    try {
      const response = await fetch(`${API_BASE_URL}/machines/${id}/refresh`, {
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
      console.error(`Error refreshing machine ${id}:`, error);
      throw error;
    }
  }
}

export default new ApiService();
