import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import apiService from '../services/api';

const MachineList = () => {
  const [machines, setMachines] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [simulating, setSimulating] = useState(false);

  const fetchMachines = async () => {
    try {
      setLoading(true);
      const data = await apiService.getMachines();
      setMachines(data);
      setError(null);
    } catch (err) {
      setError('Failed to fetch machines');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSimulate = async (machineId) => {
    try {
      setSimulating(true);
      const updatedMachine = await apiService.simulateMachine(machineId);
      
      // Update the machine in the list
      setMachines(prev => prev.map(m => 
        m.id === machineId ? updatedMachine : m
      ));
      
      setError(null);
    } catch (err) {
      setError(`Failed to simulate machine ${machineId}`);
      console.error(err);
    } finally {
      setSimulating(false);
    }
  };

  const handleSimulateNew = async () => {
    try {
      setSimulating(true);
      // Simulate a new machine with ID M001, M002, etc.
      const newId = `M${String(machines.length + 1).padStart(3, '0')}`;
      const newMachine = await apiService.simulateMachine(newId);
      setMachines(prev => [...prev, newMachine]);
      setError(null);
    } catch (err) {
      setError('Failed to simulate new machine');
      console.error(err);
    } finally {
      setSimulating(false);
    }
  };

  useEffect(() => {
    fetchMachines();
    
    // Auto-refresh every 5 seconds
    const interval = setInterval(fetchMachines, 5000);
    
    return () => clearInterval(interval);
  }, []);

  const getStatusColor = (status) => {
    switch (status) {
      case 'Çalışıyor':
        return 'bg-green-100 text-green-800 border-green-200';
      case 'Duruşta':
        return 'bg-yellow-100 text-yellow-800 border-green-200';
      case 'Alarm':
        return 'bg-red-100 text-red-800 border-red-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  const getStatusIcon = (status) => {
    switch (status) {
      case 'Çalışıyor':
        return '✅';
      case 'Duruşta':
        return '🟡';
      case 'Alarm':
        return '🔴';
      default:
        return '❓';
    }
  };

  if (loading && machines.length === 0) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold text-gray-900">CNC Machines</h1>
        <button
          onClick={handleSimulateNew}
          disabled={simulating}
          className="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-medium py-2 px-4 rounded-lg transition-colors"
        >
          {simulating ? 'Simulating...' : 'Simulate New Machine'}
        </button>
      </div>

      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      )}

      <div className="bg-white shadow-md rounded-lg overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Machine ID
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Production Count
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Cycle Time (s)
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Last Updated
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {machines.map((machine) => (
                <tr key={machine.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {machine.id}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${getStatusColor(machine.status)}`}>
                      {getStatusIcon(machine.status)} {machine.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {machine.productionCount}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {machine.cycleTimeSeconds}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {new Date(machine.timestamp).toLocaleString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                    <button
                      onClick={() => handleSimulate(machine.id)}
                      disabled={simulating}
                      className="text-green-600 hover:text-green-900 disabled:text-gray-400"
                    >
                      {simulating ? 'Simulating...' : 'Simulate'}
                    </button>
                    <Link
                      to={`/machine/${machine.id}`}
                      className="text-blue-600 hover:text-blue-900"
                    >
                      View Details
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {machines.length === 0 && !loading && (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">No machines found. Click "Simulate New Machine" to add one.</p>
        </div>
      )}
    </div>
  );
};

export default MachineList;
