export default function AboutUs() {
    return (
        <div className="flex h-full flex-col">
            <div className="mr-4 ml-4 flex-1 p-8">
                <p className="text-lg font-bold">Hi there!</p>
                <p className="mt-4">
                    This is a small project used to study, created by William Thai. The purpose is to demonstrate the functionality of our BookStore application and to learn how to become a fullstack
                    developer. if you have any suggestion to improve my work, please feel free to contact me via email as{" "}
                    <a href="mailto:chuongthai885@gmail.com" className="text-blue-600 hover:underline">
                        chuongthai885@gmail.com
                    </a>
                    .
                </p>
                <p className="mt-4">Thank you for visiting our project!</p>
                <p className="mt-4 text-right">
                    Regard,
                    <br />
                    William
                </p>
            </div>
            <footer className="bg-rose-200 p-2 p-4 text-center shadow-md">&copy; {new Date().getFullYear()} William Thai. All rights reserved.</footer>
        </div>
    );
}
