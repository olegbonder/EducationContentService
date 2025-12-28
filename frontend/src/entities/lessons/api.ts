import axios from "axios";

export const lessonsApi = {
  getLessons: () => {
    const response = axios.get("http://localhost:5238/api");
  },
};
