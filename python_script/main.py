import numpy as np
from qiskit import QuantumCircuit, execute, Aer
from flask import Flask, jsonify
import threading
import time

app = Flask(__name__)

def quantum_random_float(n=10):
    # Create a quantum circuit with n qubits
    qc = QuantumCircuit(n, n)

    # Apply a Hadamard gate to put each qubit in a superposition state
    for i in range(n):
        qc.h(i)

    # Measure each qubit
    qc.measure(range(n), range(n))

    # Execute the circuit on a quantum simulator
    simulator = Aer.get_backend('qasm_simulator')
    job = execute(qc, simulator, shots=1)
    result = job.result()

    # Get the counts of 0s and 1s
    counts = result.get_counts(qc)

    # The counts dictionary will only have one key, because shots=1.
    # This key is a string of 0s and 1s, representing the result of measuring each qubit.
    for key in counts:
        # Convert the binary fraction to a decimal fraction
        decimal = 0
        for i, bit in enumerate(reversed(key)):
            decimal += int(bit) / 2**(i+1)
        return decimal

def generate_quantum_noise(n):
    return np.array([quantum_random_float(10) for _ in range(n)])

def smooth(data, window_size):
    return np.convolve(data, np.ones(window_size)/window_size, mode='valid')

# Set window size for smoothing
window_size = 5

# Generate quantum noise (add window_size-1 to the length)
data = generate_quantum_noise(1000 + window_size - 1)

# Apply smoothing
smoothed_data = smooth(data, window_size)

dataToAPI = []

def update_data():
    global dataToAPI
    while True:
        # Generate quantum noise (add window_size-1 to the length)
        data = generate_quantum_noise(1000 + window_size - 1)

        # Apply smoothing
        smoothed_data = smooth(data, window_size)

        dataToAPI = []

        for i in range(len(smoothed_data)):
            dataToAPI.append(smoothed_data[i])
        
        time.sleep(5)  # pause for 10 seconds

for i in range(len(smoothed_data)):
    dataToAPI.append(smoothed_data[i])

@app.route('/api/get_noise')
def get_noise():
    return jsonify(dataToAPI)

if __name__ == '__main__':
    data_thread = threading.Thread(target=update_data)
    data_thread.start()
    app.run(debug = True)