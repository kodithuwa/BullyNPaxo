**PAXOS Consensus Algorithm for Distributed Systems**
Paxos is an algorithm that enables a distributed set of computers (for example, a cluster of distributed database nodes) to achieve consensus over an asynchronous network. 
To achieve agreement, one or more of the computers proposes a value to Paxos. 
Consensus is achieved when a majority of the computers running Paxos agrees on one of the proposed values.

Outline Approach 

Choose project based on digit sum of the Index Number - MS21930768
21930768 -> 2 + 1 + 9 + 3 + 0 + 7 + 6 + 8 = 36 => 36  => 3 + 6 => 9/2  Modulus  1 =>Use Case 1

Introduction
The course-based activity as required component to complete SE5090 subject. I have fallen to Use Case 1 – A censuses based prime number deciding in distributed system.
Bully algorithm and Paxos algorithm are implemented in this project.

Technologies
Visual Studio 2019, C#, NetMQ

Project Architecture
![image](https://github.com/user-attachments/assets/2c06132a-cb9e-4f8f-9bf8-3d73b17e9816)

Sequence Diagrams

Leader Selection
![image](https://github.com/user-attachments/assets/da308fd1-3a35-4028-8948-031ff35642e0)

Assign Roles (Leader, Proposer, Acceptor, Learner) to Nodes
![image](https://github.com/user-attachments/assets/4eee326e-4d47-4b7e-8cfb-db8961ac37d9)

Number processing by Proposer 
![image](https://github.com/user-attachments/assets/e362d040-33e7-4521-b520-b1418f5151d4)

Acceptor verification process

![image](https://github.com/user-attachments/assets/251b4622-0c76-4984-a4c9-fe10c38303f8)

Learner finalizing process
![image](https://github.com/user-attachments/assets/01c51362-85a7-43ac-8a83-2d0164a75fc1)


**Implementation**
![image](https://github.com/user-attachments/assets/21068909-53aa-47e3-a77a-9817d2fc5cdc)

Node 1 to Note 5 implement as distributed nodes

![image](https://github.com/user-attachments/assets/2df17382-2a73-4484-86b9-0aef3351662c)
App.config of each node keeps all endpoints as a service registry

Select a New Leader
![image](https://github.com/user-attachments/assets/05aafd5b-da24-4d34-83a7-f08f2e6225b3)

Final Result from the Learner
![image](https://github.com/user-attachments/assets/257a2136-0fc1-42f1-86d0-193a1a9e70b6)



