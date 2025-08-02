require("dotenv").config()
const jwt = require("jsonwebtoken");
const bcrypt = require("bcrypt");
const User = require("../models/user");
const ServiceResponse = require("../generics/ServiceResponse");

exports.createUser = async (req, res) => {
    try {
        const { email, password } = req.body;

        if (!email || !password) {
            return ServiceResponse.failure("Please provide email and password");
        }

        // Check if user already exists
        const existingUser = await User.findOne({ where: { email } });
        if (existingUser) {
            return ServiceResponse.failure("User already exists");
        }

        // Hash password before saving
        const hashedPassword = await bcrypt.hash(password, 10);

        const user = await User.create({
            email,
            password//: hashedPassword,
        });

        return ServiceResponse.success("Successfully created user", {
            id: user.id,
            email: user.email,
        });
    } catch (error) {
        console.log(error);
        return ServiceResponse.failure("Error occurred while creating user");
    }
};

exports.login = async (req, res) => {
    try {
        const { email, password } = req.body;

        if (!email || !password) {
            return ServiceResponse.failure("Enter email and password");
        }

        const user = await User.findOne({ where: { email } });
        if (!user) {
            return ServiceResponse.failure("Email not found");
        }

        const isPasswordValid = await bcrypt.compare(password, user.password);
        if (!isPasswordValid) {
            return ServiceResponse.failure("Invalid password");
        }

        const token = jwt.sign(
            {
                id: user.id,
                email: user.email,
            },
            process.env.JWT_SECRET,
            {
                expiresIn: "1h",
            }
        );

        return ServiceResponse.success('success',"Login successful", token);
    } catch (error) {
        console.log(error);
        return ServiceResponse.failure("Error occurred while logging in");
    }
};
