import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar';
import MachineList from './components/MachineList';
import MachineDetail from './components/MachineDetail';

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <main className="container mx-auto px-4 py-8">
          <Routes>
            <Route path="/" element={<MachineList />} />
            <Route path="/machine/:id" element={<MachineDetail />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;
