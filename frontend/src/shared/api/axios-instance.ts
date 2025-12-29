import axios from "axios";

export const apiClient = axios.create({
  baseURL: "http://localhost:5238/api/",
  headers: {
    "Content-Type": "application/json",
  },
});
