import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import apiService from '../services/api';

const MachineDetail = () => {
  const { id } = useParams();
  const [machine, setMachine] = useState(null);
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [simulating, setSimulating] = useState(false);
  const [refreshing, setRefreshing] = useState(false);

  const fetchMachine = async () => {
    try {
      setLoading(true);
      const data = await apiService.getMachineById(id);
      setMachine(data);
      setError(null);
    } catch (err) {
      setError('Failed to fetch machine details');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchHistory = async () => {
    try {
      const data = await apiService.getMachineHistory(id, 15); // Last 15 minutes
      setHistory(data.samples || []);
    } catch (err) {
      console.error('Failed to fetch history:', err);
      // Don't set error for history, just log it
    }
  };

  const handleSimulate = async () => {
    try {
      setSimulating(true);
      const updatedMachine = await apiService.simulateMachine(id);
      setMachine(updatedMachine);
      
      // Refresh history after simulation
      await fetchHistory();
      
      setError(null);
    } catch (err) {
      setError('Failed to simulate machine');
      console.error(err);
    } finally {
      setSimulating(false);
    }
  };

  const handleRefresh = async () => {
    try {
      setRefreshing(true);
      const updatedMachine = await apiService.refreshMachine(id);
      setMachine(updatedMachine);
      
      // Refresh history after refresh
      await fetchHistory();
      
      setError(null);
    } catch (err) {
      setError('Failed to refresh machine data');
      console.error(err);
    } finally {
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchMachine();
    fetchHistory();
    
    // Auto-refresh every 5 seconds
    const interval = setInterval(() => {
      fetchMachine();
      fetchHistory();
    }, 5000);
    
    return () => clearInterval(interval);
  }, [id]);

  const getStatusColor = (status) => {
    switch (status) {
      case 'Çalışıyor':
        return 'bg-green-100 text-green-800 border-green-200';
      case 'Duruşta':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
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

  // Prepare chart data from real history
  const chartData = history.map(sample => ({
    time: new Date(sample.timestamp).toLocaleTimeString(),
    productionCount: sample.productionCount,
    cycleTime: sample.cycleTimeSeconds
  })).reverse(); // Show newest data on the right

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-4">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
        <Link
          to="/"
          className="text-blue-600 hover:text-blue-900"
        >
          ← Back to Machine List
        </Link>
      </div>
    );
  }

  if (!machine) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500 text-lg">Machine not found.</p>
        <Link
          to="/"
          className="text-blue-600 hover:text-blue-900 mt-4 inline-block"
        >
          ← Back to Machine List
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Link
            to="/"
            className="text-blue-600 hover:text-blue-900"
          >
            ← Back to Machine List
          </Link>
          <h1 className="text-3xl font-bold text-gray-900">Machine {machine.id}</h1>
        </div>
        
        <div className="flex space-x-2">
          <button
            onClick={handleSimulate}
            disabled={simulating}
            className="bg-green-600 hover:bg-green-700 disabled:bg-green-400 text-white font-medium py-2 px-4 rounded-lg transition-colors"
          >
            {simulating ? 'Simulating...' : 'Simulate'}
          </button>
          <button
            onClick={handleRefresh}
            disabled={refreshing}
            className="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-medium py-2 px-4 rounded-lg transition-colors"
          >
            {refreshing ? 'Refreshing...' : 'Refresh'}
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Machine Details Card */}
        <div className="bg-white shadow-md rounded-lg p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Machine Information</h2>
          
          <div className="space-y-4">
            <div className="flex justify-between items-center">
              <span className="text-gray-600">Machine ID:</span>
              <span className="font-medium">{machine.id}</span>
            </div>
            
            <div className="flex justify-between items-center">
              <span className="text-gray-600">Status:</span>
              <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${getStatusColor(machine.status)}`}>
                {getStatusIcon(machine.status)} {machine.status}
              </span>
            </div>
            
            <div className="flex justify-between items-center">
              <span className="text-gray-600">Production Count:</span>
              <span className="font-medium">{machine.productionCount}</span>
            </div>
            
            <div className="flex justify-between items-center">
              <span className="text-gray-600">Cycle Time:</span>
              <span className="font-medium">{machine.cycleTimeSeconds} seconds</span>
            </div>
            
            <div className="flex justify-between items-center">
              <span className="text-gray-600">Last Updated:</span>
              <span className="font-medium">{new Date(machine.timestamp).toLocaleString()}</span>
            </div>
          </div>
        </div>

        {/* Status Summary Card */}
        <div className="bg-white shadow-md rounded-lg p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Status Summary</h2>
          
          <div className="space-y-4">
            <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div className="flex items-center space-x-3">
                <div className="w-3 h-3 bg-green-500 rounded-full"></div>
                <span className="text-gray-700">Working</span>
              </div>
              <span className="font-medium text-gray-900">
                {machine.status === 'Çalışıyor' ? 'Active' : 'Inactive'}
              </span>
            </div>
            
            <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div className="flex items-center space-x-3">
                <div className="w-3 h-3 bg-blue-500 rounded-full"></div>
                <span className="text-gray-700">Production Rate</span>
              </div>
              <span className="font-medium text-gray-900">
                {machine.cycleTimeSeconds > 0 ? Math.round(3600 / machine.cycleTimeSeconds) : 0} parts/hour
              </span>
            </div>
            
            <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div className="flex items-center space-x-3">
                <div className="w-3 h-3 bg-purple-500 rounded-full"></div>
                <span className="text-gray-700">Efficiency</span>
              </div>
              <span className="font-medium text-gray-900">
                {machine.status === 'Çalışıyor' ? '100%' : '0%'}
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Production Chart */}
      <div className="bg-white shadow-md rounded-lg p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Production Count Over Time (Last 15 minutes)</h2>
        
        {chartData.length > 0 ? (
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="time" />
                <YAxis />
                <Tooltip />
                <Line 
                  type="monotone" 
                  dataKey="productionCount" 
                  stroke="#3B82F6" 
                  strokeWidth={2}
                  dot={{ fill: '#3B82F6', strokeWidth: 2, r: 4 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        ) : (
          <div className="h-64 flex items-center justify-center text-gray-500">
            No history data available yet. Try simulating the machine to generate data.
          </div>
        )}
      </div>

      {/* History Table */}
      {history.length > 0 && (
        <div className="bg-white shadow-md rounded-lg p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Recent History</h2>
          
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Time</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Production Count</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Cycle Time (s)</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {history.map((sample, index) => (
                  <tr key={index} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {new Date(sample.timestamp).toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${getStatusColor(sample.status)}`}>
                        {getStatusIcon(sample.status)} {sample.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {sample.productionCount}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {sample.cycleTimeSeconds}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default MachineDetail;
